using System.Reflection;
using System.Text.Json;
using InfluenciAI.Infrastructure.Data;
using MediatR;
using InfluenciAI.Application.Tenants;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using FluentValidation;
using InfluenciAI.Application.Common.Behaviors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using RabbitMQ.Client;
using InfluenciAI.Api.Health;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Identity;
using InfluenciAI.Infrastructure.Identity;
using InfluenciAI.Api.Cors;
using InfluenciAI.Api.Startup;
using Microsoft.AspNetCore.Cors.Infrastructure;
using System.Security.Cryptography;
using InfluenciAI.Application.Common.Interfaces;
using InfluenciAI.Infrastructure.Services;
using InfluenciAI.Application.SocialProfiles.Commands;
using InfluenciAI.Application.SocialProfiles.Queries;
using InfluenciAI.Application.Content.Commands;
using InfluenciAI.Application.Content.Queries;
using InfluenciAI.Domain.Entities;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration (Console sink for dev)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Services
var enableSwagger = builder.Configuration.GetValue("Swagger:Enabled", true);
builder.Services.AddEndpointsApiExplorer();
if (enableSwagger)
{
    try
    {
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below."
            });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }
    catch (TypeLoadException)
    {
        // In integration tests or mismatched package versions, skip Swagger
    }
}
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<CreateTenantCommand>();
    cfg.RegisterServicesFromAssembly(typeof(ApplicationDbContext).Assembly);
});
builder.Services.AddValidatorsFromAssemblyContaining<CreateTenantValidator>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// DbContext: PostgreSQL when configured; fallback to InMemory (tests)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("InfluenciAI_Api_Tests"));
}
builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

// Application Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ITwitterService, TwitterService>();
builder.Services.AddScoped<ITwitterOAuthService, TwitterOAuthService>();
builder.Services.AddSingleton<ITokenProtectionService, TokenProtectionService>();

// Data Protection for token encryption
builder.Services.AddDataProtection();

// HttpClient for OAuth
builder.Services.AddHttpClient<ITwitterOAuthService, TwitterOAuthService>();

// Identity
builder.Services.AddIdentityCore<AppUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Health checks
var hcBuilder = builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" });
if (!string.IsNullOrWhiteSpace(connectionString))
{
    hcBuilder.AddNpgSql(connectionString!, tags: new[] { "ready" });
}
var redisConn = builder.Configuration["Redis:ConnectionString"];
if (!string.IsNullOrWhiteSpace(redisConn))
{
    hcBuilder.AddRedis(redisConn, name: "redis", tags: new[] { "ready" });
}
var rabbitConn = builder.Configuration["RabbitMQ:ConnectionString"];
if (!string.IsNullOrWhiteSpace(rabbitConn))
{
    hcBuilder.AddCheck("rabbitmq", new RabbitMqTcpHealthCheck(rabbitConn), tags: new[] { "ready" });
}

// OpenTelemetry (console exporter for dev)
builder.Services.AddOpenTelemetry()
    .ConfigureResource(rb => rb.AddService("InfluenciAI.Api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddConsoleExporter());

builder.Services.AddHttpClient<ITwitterService, TwitterService>()
    .AddTransientHttpErrorPolicy(policy =>
        policy.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
    .AddTransientHttpErrorPolicy(policy =>
        policy.CircuitBreakerAsync(5, TimeSpan.FromMinutes(1)));

// AuthN/AuthZ (JWT - dev skeleton)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "")),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TenantAdmin", policy => policy.RequireRole("admin"));
});

// CORS (dynamic provider)
builder.Services.AddCors();
builder.Services.AddSingleton<ICorsPolicyProvider, TenantCorsPolicyProvider>();

// HealthChecks UI - TEMPORARILY DISABLED due to EF Core 10 RC incompatibility
// builder.Services.AddHealthChecksUI().AddInMemoryStorage();
builder.Services.AddHostedService<DataSeederHostedService>();
builder.Services.AddHostedService<DataCollectorService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Health endpoints
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// HealthChecks UI - TEMPORARILY DISABLED due to EF Core 10 RC incompatibility
// app.MapHealthChecksUI(options =>
// {
//     options.UIPath = "/health/ui";
// });

// Version endpoint
app.MapGet("/version", () => new
{
    name = "InfluenciAI.Api",
    version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "dev",
    environment = app.Environment.EnvironmentName
});

// Tenants endpoints (minimal CRUD subset)
app.MapGet("/api/tenants", async (IMediator mediator, CancellationToken ct)
    => Results.Ok(await mediator.Send(new ListTenantsQuery(), ct))).RequireAuthorization("TenantAdmin");

app.MapGet("/api/tenants/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    var dto = await mediator.Send(new GetTenantByIdQuery(id), ct);
    return dto is null ? Results.NotFound() : Results.Ok(dto);
}).RequireAuthorization("TenantAdmin");
// Users by tenant
app.MapGet("/api/tenants/{tenantId:guid}/users", async (Guid tenantId, UserManager<InfluenciAI.Infrastructure.Identity.AppUser> userManager, System.Security.Claims.ClaimsPrincipal user) =>
{
    if (!Guid.TryParse(user.FindFirstValue("tenant"), out var userTenant) || userTenant != tenantId) return Results.Forbid();
    var users = userManager.Users.Where(u => u.TenantId == tenantId)
        .Select(u => new { u.Id, u.Email, u.DisplayName, u.TenantId });
    return Results.Ok(await users.ToListAsync());
}).RequireAuthorization("TenantAdmin");

app.MapPost("/api/tenants/{tenantId:guid}/users", async (Guid tenantId, CreateUserRequest req, UserManager<InfluenciAI.Infrastructure.Identity.AppUser> userManager, ITenantRepository tenants, System.Security.Claims.ClaimsPrincipal principal, CancellationToken ct) =>
{
    if (!Guid.TryParse(principal.FindFirstValue("tenant"), out var userTenant) || userTenant != tenantId) return Results.Forbid();
    if (tenantId == Guid.Empty) return Results.BadRequest(new { error = "TenantId required" });
    var tenant = await tenants.GetByIdAsync(tenantId, ct);
    if (tenant is null) return Results.BadRequest(new { error = "Invalid TenantId" });

    var entity = new InfluenciAI.Infrastructure.Identity.AppUser
    {
        UserName = req.Email,
        Email = req.Email,
        TenantId = tenantId,
        DisplayName = req.DisplayName ?? string.Empty,
        EmailConfirmed = true
    };
    var result = await userManager.CreateAsync(entity, req.Password);
    if (!result.Succeeded) return Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });
    return Results.Created($"/api/tenants/{tenantId}/users/{entity.Id}", new { entity.Id, entity.Email, entity.DisplayName, entity.TenantId });
}).RequireAuthorization("TenantAdmin");

app.MapPut("/api/tenants/{tenantId:guid}/users/{id}", async (Guid tenantId, string id, UpdateUserRequest req, UserManager<InfluenciAI.Infrastructure.Identity.AppUser> userManager, System.Security.Claims.ClaimsPrincipal principal) =>
{
    if (!Guid.TryParse(principal.FindFirstValue("tenant"), out var userTenant) || userTenant != tenantId) return Results.Forbid();
    var entity = await userManager.FindByIdAsync(id);
    if (entity is null || entity.TenantId != tenantId) return Results.NotFound();
    entity.DisplayName = req.DisplayName ?? entity.DisplayName;
    if (!string.IsNullOrWhiteSpace(req.Email))
    {
        entity.Email = req.Email;
        entity.UserName = req.Email;
    }
    var result = await userManager.UpdateAsync(entity);
    return result.Succeeded ? Results.Ok(new { entity.Id, entity.Email, entity.DisplayName }) : Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });
}).RequireAuthorization("TenantAdmin");

app.MapDelete("/api/tenants/{tenantId:guid}/users/{id}", async (Guid tenantId, string id, UserManager<InfluenciAI.Infrastructure.Identity.AppUser> userManager, System.Security.Claims.ClaimsPrincipal principal) =>
{
    if (!Guid.TryParse(principal.FindFirstValue("tenant"), out var userTenant) || userTenant != tenantId) return Results.Forbid();
    var entity = await userManager.FindByIdAsync(id);
    if (entity is null || entity.TenantId != tenantId) return Results.NotFound();
    var result = await userManager.DeleteAsync(entity);
    return result.Succeeded ? Results.NoContent() : Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });
}).RequireAuthorization("TenantAdmin");

app.MapPost("/api/tenants", async (CreateTenantRequest req, IMediator mediator, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.Name)) return Results.BadRequest(new { error = "Name is required" });
    var dto = await mediator.Send(new CreateTenantCommand(req.Name), ct);
    return Results.Created($"/api/tenants/{dto.Id}", dto);
}).RequireAuthorization("TenantAdmin");

app.MapPut("/api/tenants/{id:guid}", async (Guid id, UpdateTenantRequest req, IMediator mediator, CancellationToken ct) =>
{
    try
    {
        var dto = await mediator.Send(new UpdateTenantCommand(id, req.Name), ct);
        return Results.Ok(dto);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
}).RequireAuthorization("TenantAdmin");

app.MapDelete("/api/tenants/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    var ok = await mediator.Send(new DeleteTenantCommand(id), ct);
    return ok ? Results.NoContent() : Results.NotFound();
}).RequireAuthorization("TenantAdmin");

// Auth endpoints (register/login)
app.MapPost("/auth/register", async (RegisterRequest req, UserManager<AppUser> userManager, ITenantRepository tenants, System.Security.Claims.ClaimsPrincipal principal, CancellationToken ct) =>
{
    if (!Guid.TryParse(principal.FindFirstValue("tenant"), out var userTenant)) return Results.Forbid();
    if (req.TenantId == Guid.Empty || req.TenantId != userTenant)
        return Results.BadRequest(new { error = "Invalid or mismatched TenantId" });
    var tenant = await tenants.GetByIdAsync(req.TenantId, ct);
    if (tenant is null) return Results.BadRequest(new { error = "Invalid TenantId" });

    var user = new AppUser
    {
        UserName = req.Email,
        Email = req.Email,
        TenantId = req.TenantId,
        DisplayName = req.DisplayName ?? string.Empty,
        EmailConfirmed = true
    };

    var result = await userManager.CreateAsync(user, req.Password);
    if (!result.Succeeded) return Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });
    return Results.Created($"/api/users/{user.Id}", new { user.Id, user.Email, user.DisplayName, user.TenantId });
}).RequireAuthorization("TenantAdmin");

// Role management (admin only)
app.MapGet("/auth/roles", (RoleManager<IdentityRole> roleManager) =>
{
    var roles = roleManager.Roles.Select(r => r.Name).ToList();
    return Results.Ok(roles);
}).RequireAuthorization("TenantAdmin");

app.MapPost("/auth/roles", async (CreateRoleRequest req, RoleManager<IdentityRole> roleManager) =>
{
    if (string.IsNullOrWhiteSpace(req.Name)) return Results.BadRequest(new { error = "Name is required" });
    if (await roleManager.RoleExistsAsync(req.Name)) return Results.Conflict(new { error = "Role already exists" });
    var res = await roleManager.CreateAsync(new IdentityRole(req.Name));
    return res.Succeeded ? Results.Created($"/auth/roles/{req.Name}", new { name = req.Name }) : Results.BadRequest(new { errors = res.Errors.Select(e => e.Description) });
}).RequireAuthorization("TenantAdmin");

app.MapGet("/auth/users/{id}/roles", async (string id, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, System.Security.Claims.ClaimsPrincipal principal) =>
{
    var user = await userManager.FindByIdAsync(id);
    if (user is null) return Results.NotFound();
    if (!Guid.TryParse(principal.FindFirstValue("tenant"), out var tenant) || tenant != user.TenantId) return Results.Forbid();
    var roles = await userManager.GetRolesAsync(user);
    return Results.Ok(roles);
}).RequireAuthorization("TenantAdmin");

app.MapPost("/auth/users/{id}/roles", async (string id, AddRoleRequest req, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, System.Security.Claims.ClaimsPrincipal principal) =>
{
    var user = await userManager.FindByIdAsync(id);
    if (user is null) return Results.NotFound();
    if (!Guid.TryParse(principal.FindFirstValue("tenant"), out var tenant) || tenant != user.TenantId) return Results.Forbid();
    if (!await roleManager.RoleExistsAsync(req.Role)) return Results.BadRequest(new { error = "Role not found" });
    var res = await userManager.AddToRoleAsync(user, req.Role);
    return res.Succeeded ? Results.NoContent() : Results.BadRequest(new { errors = res.Errors.Select(e => e.Description) });
}).RequireAuthorization("TenantAdmin");

app.MapDelete("/auth/users/{id}/roles/{role}", async (string id, string role, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, System.Security.Claims.ClaimsPrincipal principal) =>
{
    var user = await userManager.FindByIdAsync(id);
    if (user is null) return Results.NotFound();
    if (!Guid.TryParse(principal.FindFirstValue("tenant"), out var tenant) || tenant != user.TenantId) return Results.Forbid();
    if (!await roleManager.RoleExistsAsync(role)) return Results.BadRequest(new { error = "Role not found" });
    var res = await userManager.RemoveFromRoleAsync(user, role);
    return res.Succeeded ? Results.NoContent() : Results.BadRequest(new { errors = res.Errors.Select(e => e.Description) });
}).RequireAuthorization("TenantAdmin");

app.MapPost("/auth/login", async (LoginRequest req, UserManager<AppUser> userManager, IConfiguration cfg, ApplicationDbContext db) =>
{
    var user = await userManager.FindByEmailAsync(req.Email);
    if (user is null) return Results.Unauthorized();
    var valid = await userManager.CheckPasswordAsync(user, req.Password);
    if (!valid) return Results.Unauthorized();

    var issuer = cfg["Jwt:Issuer"] ?? "InfluenciAI";
    var audience = cfg["Jwt:Audience"] ?? "InfluenciAI.Client";
    var key = cfg["Jwt:Key"] ?? "dev-secret-change-me-please-very-long";

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? "user"),
        new Claim("tenant", user.TenantId.ToString())
    };

    // add role claims
    var roles = await userManager.GetRolesAsync(user);
    foreach (var role in roles)
    {
        claims.Add(new Claim(ClaimTypes.Role, role));
    }

    var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
    var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(8),
        signingCredentials: creds);

    var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);

    var refreshPlain = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    var refreshHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshPlain)));
    db.RefreshTokens.Add(new RefreshToken
    {
        UserId = user.Id,
        TokenHash = refreshHash,
        CreatedAtUtc = DateTime.UtcNow,
        ExpiresAtUtc = DateTime.UtcNow.AddDays(14)
    });
    await db.SaveChangesAsync();

    return Results.Ok(new { access_token = jwt, refresh_token = refreshPlain });
});

app.MapPost("/auth/refresh", async (RefreshRequest req, ApplicationDbContext db, UserManager<AppUser> userManager, IConfiguration cfg, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.refresh_token)) return Results.Unauthorized();
    var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(req.refresh_token)));
    var token = await db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, ct);
    if (token is null || token.RevokedAtUtc != null || token.ExpiresAtUtc <= DateTime.UtcNow)
        return Results.Unauthorized();

    var user = await userManager.FindByIdAsync(token.UserId);
    if (user is null) return Results.Unauthorized();

    var issuer = cfg["Jwt:Issuer"] ?? "InfluenciAI";
    var audience = cfg["Jwt:Audience"] ?? "InfluenciAI.Client";
    var key = cfg["Jwt:Key"] ?? "dev-secret-change-me-please-very-long";

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? "user"),
        new Claim("tenant", user.TenantId.ToString())
    };
    var roles = await userManager.GetRolesAsync(user);
    foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));

    var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
    var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(8),
        signingCredentials: creds);
    var access = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(jwt);

    token.RevokedAtUtc = DateTime.UtcNow;
    var newRefreshPlain = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    var newRefreshHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(newRefreshPlain)));
    db.RefreshTokens.Add(new RefreshToken
    {
        UserId = user.Id,
        TokenHash = newRefreshHash,
        CreatedAtUtc = DateTime.UtcNow,
        ExpiresAtUtc = DateTime.UtcNow.AddDays(14)
    });
    await db.SaveChangesAsync(ct);

    return Results.Ok(new { access_token = access, refresh_token = newRefreshPlain });
});

app.MapPost("/auth/logout", async (RefreshRequest req, ApplicationDbContext db, System.Security.Claims.ClaimsPrincipal principal, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.refresh_token)) return Results.BadRequest(new { error = "refresh_token required" });
    var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();
    var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(req.refresh_token)));
    var token = await db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, ct);
    if (token is null) return Results.NoContent();
    if (!string.Equals(token.UserId, userId, StringComparison.Ordinal)) return Results.Forbid();
    token.RevokedAtUtc = DateTime.UtcNow;
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
}).RequireAuthorization();

app.MapPost("/auth/logout/all", async (ApplicationDbContext db, System.Security.Claims.ClaimsPrincipal principal, CancellationToken ct) =>
{
    var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();
    var toRevoke = await db.RefreshTokens.Where(x => x.UserId == userId && x.RevokedAtUtc == null && x.ExpiresAtUtc > DateTime.UtcNow).ToListAsync(ct);
    foreach (var t in toRevoke) t.RevokedAtUtc = DateTime.UtcNow;
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
}).RequireAuthorization();

// Twitter OAuth endpoints
app.MapGet("/auth/twitter/authorize", (ITwitterOAuthService oauthService, HttpContext context) =>
{
    var callbackUrl = $"{context.Request.Scheme}://{context.Request.Host}/auth/twitter/callback";
    var authUrl = oauthService.GetAuthorizationUrl(callbackUrl);

    // In production, store code_verifier in session or distributed cache
    // For now, we'll include it in the state (NOT SECURE - just for MVP)
    var (codeVerifier, _) = oauthService.GeneratePkceChallenge();
    context.Response.Cookies.Append("twitter_code_verifier", codeVerifier, new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        MaxAge = TimeSpan.FromMinutes(10)
    });

    return Results.Redirect(authUrl);
}).RequireAuthorization();

app.MapGet("/auth/twitter/callback", async (
    string code,
    string state,
    ITwitterOAuthService oauthService,
    IMediator mediator,
    HttpContext context,
    CancellationToken ct) =>
{
    try
    {
        // Retrieve code verifier from cookie
        if (!context.Request.Cookies.TryGetValue("twitter_code_verifier", out var codeVerifier))
        {
            return Results.BadRequest(new { error = "Code verifier not found. Please try again." });
        }

        // Exchange code for tokens
        var tokens = await oauthService.ExchangeCodeForTokensAsync(code, codeVerifier);

        // Connect the social profile
        var command = new ConnectSocialProfileCommand(
            SocialNetwork.Twitter,
            tokens.AccessToken,
            tokens.RefreshToken,
            DateTime.UtcNow.AddSeconds(tokens.ExpiresIn)
        );

        var profile = await mediator.Send(command, ct);

        // Clear the cookie
        context.Response.Cookies.Delete("twitter_code_verifier");

        // Redirect to success page or return JSON
        return Results.Ok(new
        {
            success = true,
            message = "Twitter account connected successfully",
            profile
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).RequireAuthorization();

// Social Profiles endpoints
app.MapPost("/api/social-profiles", async (ConnectSocialProfileCommand cmd, IMediator mediator)
    => Results.Ok(await mediator.Send(cmd)))
    .RequireAuthorization();

app.MapGet("/api/social-profiles", async (IMediator mediator, CancellationToken ct)
    => Results.Ok(await mediator.Send(new ListSocialProfilesQuery(), ct)))
    .RequireAuthorization();

// Content endpoints
app.MapPost("/api/content", async (CreateContentCommand cmd, IMediator mediator)
    => Results.Ok(await mediator.Send(cmd)))
    .RequireAuthorization();

app.MapGet("/api/content", async (IMediator mediator, CancellationToken ct)
    => Results.Ok(await mediator.Send(new ListContentQuery(), ct)))
    .RequireAuthorization();

app.MapPost("/api/content/{id:guid}/publish", async (Guid id, PublishContentRequest req, IMediator mediator)
    => Results.Ok(await mediator.Send(new PublishContentCommand(id, req.SocialProfileId))))
    .RequireAuthorization();

app.MapGet("/api/content/{id:guid}/metrics", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    try
    {
        var metrics = await mediator.Send(new InfluenciAI.Application.Metrics.Queries.GetContentMetricsQuery(id), ct);
        return Results.Ok(metrics);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).RequireAuthorization();

app.Run();

public record CreateTenantRequest(string Name);
public record UpdateTenantRequest(string Name);
public record RegisterRequest(Guid TenantId, string Email, string Password, string? DisplayName);
public record LoginRequest(string Email, string Password);
public record CreateRoleRequest(string Name);
public record AddRoleRequest(string Role);
public record CreateUserRequest(string Email, string Password, string? DisplayName);
public record UpdateUserRequest(string? Email, string? DisplayName);
public record RefreshRequest(string refresh_token);
public record ConnectSocialProfileRequest(InfluenciAI.Domain.Entities.SocialNetwork Network, string AccessToken, string? RefreshToken, DateTime TokenExpiresAt);
public record CreateContentRequest(string Title, string Body, InfluenciAI.Domain.Entities.ContentType Type);
public record PublishContentRequest(Guid SocialProfileId);

public partial class Program { }

















