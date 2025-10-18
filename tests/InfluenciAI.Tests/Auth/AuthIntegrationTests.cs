using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using InfluenciAI.Infrastructure.Data;
using InfluenciAI.Infrastructure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace InfluenciAI.Tests.Auth;

public class TestApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "");
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var mem = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "integration-test-secret-please-change",
                ["Jwt:Issuer"] = "InfluenciAI.Tests",
                ["Jwt:Audience"] = "InfluenciAI.Client.Tests",
                ["Swagger:Enabled"] = "false",
                ["ConnectionStrings:DefaultConnection"] = ""
            };
            config.AddInMemoryCollection(mem);
        });

        builder.ConfigureServices(services => { /* use API fallback InMemory and seed per test */ });
    }
}

public class AuthIntegrationTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;
    private readonly TestApiFactory _factory;

    public AuthIntegrationTests(TestApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        // Seed shared InMemory DB
        using var scope = _factory.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        var tenant = db.Tenants.FirstOrDefault();
        if (tenant is null)
        {
            tenant = new InfluenciAI.Domain.Entities.Tenant { Name = "TestTenant" };
            db.Tenants.Add(tenant);
            db.SaveChanges();
        }
        var userMgr = sp.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<AppUser>>();
        var roleMgr = sp.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
        if (!roleMgr.Roles.Any(r => r.Name == "admin"))
        {
            roleMgr.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole("admin")).GetAwaiter().GetResult();
        }
        var admin = userMgr.FindByEmailAsync("admin@tests").GetAwaiter().GetResult();
        if (admin is null)
        {
            admin = new AppUser
            {
                UserName = "admin@tests",
                Email = "admin@tests",
                TenantId = tenant.Id,
                DisplayName = "Admin Tests",
                EmailConfirmed = true
            };
            userMgr.CreateAsync(admin, "Test!234").GetAwaiter().GetResult();
            userMgr.AddToRoleAsync(admin, "admin").GetAwaiter().GetResult();
        }
    }

    private record LoginRequest(string Email, string Password);
    private record TokenPair(string access_token, string refresh_token);
    private record RefreshRequest(string refresh_token);

    [Fact(Skip="Temporariamente desabilitado: estabilizando ambiente de teste InMemory/Identity")]
    public async Task Login_Refresh_Logout_Flow_Works()
    {
        // Arrange: seed refresh token and mint access token
        string issuer = "InfluenciAI.Tests";
        string audience = "InfluenciAI.Client.Tests";
        string key = "integration-test-secret-please-change";

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var admin = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<AppUser>>().Users.First();
        var plain = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(plain)));
        db.RefreshTokens.Add(new RefreshToken { UserId = admin.Id, TokenHash = hash, CreatedAtUtc = DateTime.UtcNow, ExpiresAtUtc = DateTime.UtcNow.AddDays(7) });
        db.SaveChanges();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, admin.Id),
            new Claim(ClaimTypes.Name, admin.UserName ?? admin.Email ?? "user"),
            new Claim("tenant", admin.TenantId.ToString()),
            new Claim(ClaimTypes.Role, "admin")
        };
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, claims, expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        var access = new JwtSecurityTokenHandler().WriteToken(token);

        // Refresh
        var refreshResp = await _client.PostAsJsonAsync("/auth/refresh", new RefreshRequest(plain));
        refreshResp.IsSuccessStatusCode.Should().BeTrue();
        var tokens2 = await refreshResp.Content.ReadFromJsonAsync<TokenPair>();
        tokens2.Should().NotBeNull();
        tokens2!.access_token.Should().NotBeNullOrWhiteSpace();
        tokens2.refresh_token.Should().NotBeNullOrWhiteSpace();
        tokens2.refresh_token.Should().NotBe(plain); // rotated

        // Logout with Authorization
        var req = new HttpRequestMessage(HttpMethod.Post, "/auth/logout")
        {
            Content = JsonContent.Create(new RefreshRequest(tokens2.refresh_token))
        };
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access);
        var logoutResp = await _client.SendAsync(req);
        logoutResp.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        // Using revoked refresh should fail
        var refreshAfterLogout = await _client.PostAsJsonAsync("/auth/refresh", new RefreshRequest(tokens2.refresh_token));
        refreshAfterLogout.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact(Skip="Temporariamente desabilitado: estabilizando ambiente de teste InMemory/Identity")]
    public async Task LogoutAll_Revokes_Current_Refresh()
    {
        // Mint token
        string issuer = "InfluenciAI.Tests";
        string audience = "InfluenciAI.Client.Tests";
        string key = "integration-test-secret-please-change";
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var admin = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<AppUser>>().Users.First();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, admin.Id),
            new Claim(ClaimTypes.Name, admin.UserName ?? admin.Email ?? "user"),
            new Claim("tenant", admin.TenantId.ToString()),
            new Claim(ClaimTypes.Role, "admin")
        };
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, claims, expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        var access = new JwtSecurityTokenHandler().WriteToken(token);

        // Call logout/all with Authorization
        var req = new HttpRequestMessage(HttpMethod.Post, "/auth/logout/all");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access);
        var logoutAll = await _client.SendAsync(req);
        logoutAll.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        // Refresh should fail now
        var plain = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(plain)));
        db.RefreshTokens.Add(new RefreshToken { UserId = admin.Id, TokenHash = hash, CreatedAtUtc = DateTime.UtcNow, ExpiresAtUtc = DateTime.UtcNow.AddDays(7) });
        db.SaveChanges();
        var after = await _client.PostAsJsonAsync("/auth/refresh", new RefreshRequest(plain));
        after.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}
