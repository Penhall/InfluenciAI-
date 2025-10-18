using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using InfluenciAI.Infrastructure.Data;

namespace InfluenciAI.Tests.Auth;

public class TenantUsersIntegrationTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;
    private readonly Guid _tenantId;
    private readonly TestApiFactory _factory;

    private record LoginRequest(string Email, string Password);
    private record TokenPair(string access_token, string refresh_token);
    private record CreateUserRequest(string Email, string Password, string? DisplayName);
    private record UpdateUserRequest(string? Email, string? DisplayName);

    public TenantUsersIntegrationTests(TestApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
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
        _tenantId = tenant.Id;

        var userMgr = sp.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<InfluenciAI.Infrastructure.Identity.AppUser>>();
        var roleMgr = sp.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
        if (!roleMgr.Roles.Any(r => r.Name == "admin"))
        {
            roleMgr.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole("admin")).GetAwaiter().GetResult();
        }
        var admin = userMgr.FindByEmailAsync("admin@tests").GetAwaiter().GetResult();
        if (admin is null)
        {
            admin = new InfluenciAI.Infrastructure.Identity.AppUser
            {
                UserName = "admin@tests",
                Email = "admin@tests",
                TenantId = _tenantId,
                DisplayName = "Admin Tests",
                EmailConfirmed = true
            };
            userMgr.CreateAsync(admin, "Test!234").GetAwaiter().GetResult();
            userMgr.AddToRoleAsync(admin, "admin").GetAwaiter().GetResult();
        }
    }

    [Fact(Skip="Temporariamente desabilitado: estabilizando ambiente de teste InMemory/Identity")]
    public async Task Users_CRUD_Works_With_TenantScope()
    {
        // Mint access token for seeded admin (bypassing login in tests)
        string issuer = "InfluenciAI.Tests";
        string audience = "InfluenciAI.Client.Tests";
        string key = "integration-test-secret-please-change";
        using var scope2 = _factory.Services.CreateScope();
        var sp2 = scope2.ServiceProvider;
        var userMgr2 = sp2.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<InfluenciAI.Infrastructure.Identity.AppUser>>();
        var admin2 = userMgr2.FindByEmailAsync("admin@tests").GetAwaiter().GetResult() ?? userMgr2.Users.First();
        var claims2 = new List<System.Security.Claims.Claim>
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, admin2.Id),
            new(System.Security.Claims.ClaimTypes.Name, admin2.UserName ?? admin2.Email ?? "user"),
            new("tenant", admin2.TenantId.ToString()),
            new(System.Security.Claims.ClaimTypes.Role, "admin")
        };
        var creds2 = new Microsoft.IdentityModel.Tokens.SigningCredentials(new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key)), Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);
        var token2 = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(issuer, audience, claims2, expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds2);
        var access2 = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token2);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access2);

        // List users
        var list1 = await _client.GetFromJsonAsync<List<UserDto>>($"/api/tenants/{_tenantId}/users");
        list1.Should().NotBeNull();

        // Create user
        var createResp = await _client.PostAsJsonAsync($"/api/tenants/{_tenantId}/users", new CreateUserRequest("u1@tests", "Test!234", "User 1"));
        createResp.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        var created = await createResp.Content.ReadFromJsonAsync<UserDto>();
        created.Should().NotBeNull();

        // Update user
        var updateResp = await _client.PutAsJsonAsync($"/api/tenants/{_tenantId}/users/{created!.Id}", new UpdateUserRequest("u1b@tests", "User 1B"));
        updateResp.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var updated = await updateResp.Content.ReadFromJsonAsync<UserDto>();
        updated!.Email.Should().Be("u1b@tests");
        updated.DisplayName.Should().Be("User 1B");

        // Delete user
        var del = await _client.DeleteAsync($"/api/tenants/{_tenantId}/users/{created.Id}");
        del.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }

    private sealed class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public Guid TenantId { get; set; }
    }
}

