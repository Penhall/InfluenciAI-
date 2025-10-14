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

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration (Console sink for dev)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateTenantCommand>());
builder.Services.AddValidatorsFromAssemblyContaining<CreateTenantValidator>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// DbContext (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

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

// Polly example HttpClient (placeholder)
builder.Services.AddHttpClient("default")
    .AddStandardResilienceHandler();

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
    ResponseWriter = async (c, r) =>
    {
        c.Response.ContentType = "application/json";
        var payload = JsonSerializer.Serialize(new
        {
            status = r.Status.ToString(),
            results = r.Entries.Select(e => new { name = e.Key, status = e.Value.Status.ToString() })
        });
        await c.Response.WriteAsync(payload);
    }
});

// Version endpoint
app.MapGet("/version", () => new
{
    name = "InfluenciAI.Api",
    version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "dev",
    environment = app.Environment.EnvironmentName
});

// Tenants endpoints (minimal CRUD subset)
app.MapGet("/api/tenants", async (IMediator mediator, CancellationToken ct)
    => Results.Ok(await mediator.Send(new ListTenantsQuery(), ct))).RequireAuthorization();

app.MapGet("/api/tenants/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    var dto = await mediator.Send(new GetTenantByIdQuery(id), ct);
    return dto is null ? Results.NotFound() : Results.Ok(dto);
}).RequireAuthorization();

app.MapPost("/api/tenants", async (CreateTenantRequest req, IMediator mediator, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.Name)) return Results.BadRequest(new { error = "Name is required" });
    var dto = await mediator.Send(new CreateTenantCommand(req.Name), ct);
    return Results.Created($"/api/tenants/{dto.Id}", dto);
}).RequireAuthorization();

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
}).RequireAuthorization();

app.MapDelete("/api/tenants/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    var ok = await mediator.Send(new DeleteTenantCommand(id), ct);
    return ok ? Results.NoContent() : Results.NotFound();
}).RequireAuthorization();

// Dev token endpoint (do NOT use in production)
app.MapPost("/auth/token", (LoginRequest req, IConfiguration cfg) =>
{
    if (string.IsNullOrWhiteSpace(req.Username)) return Results.BadRequest(new { error = "username required" });

    var issuer = cfg["Jwt:Issuer"] ?? "InfluenciAI";
    var audience = cfg["Jwt:Audience"] ?? "InfluenciAI.Client";
    var key = cfg["Jwt:Key"] ?? "dev-secret-change-me-please-very-long";

    var claims = new[]
    {
        new Claim(ClaimTypes.Name, req.Username),
        new Claim("role", "dev")
    };

    var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
    var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(8),
        signingCredentials: creds);

    var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(new { access_token = jwt });
});

app.Run();

record CreateTenantRequest(string Name);
record UpdateTenantRequest(string Name);
record LoginRequest(string Username);

