using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace InfluenciAI.Desktop.Services.Users;

public interface IUsersService
{
    Task<List<UserDto>> ListAsync(Guid tenantId, CancellationToken ct);
    Task<UserDto> CreateAsync(Guid tenantId, string email, string password, string? displayName, CancellationToken ct);
    Task<UserDto> UpdateAsync(Guid tenantId, string id, string? email, string? displayName, CancellationToken ct);
    Task<bool> DeleteAsync(Guid tenantId, string id, CancellationToken ct);
}

public sealed class UsersService(HttpClient http) : IUsersService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<List<UserDto>> ListAsync(Guid tenantId, CancellationToken ct)
        => await http.GetFromJsonAsync<List<UserDto>>($"/api/tenants/{tenantId}/users", JsonOptions, ct) ?? new List<UserDto>();

    public async Task<UserDto> CreateAsync(Guid tenantId, string email, string password, string? displayName, CancellationToken ct)
    {
        var resp = await http.PostAsJsonAsync($"/api/tenants/{tenantId}/users", new { Email = email, Password = password, DisplayName = displayName }, JsonOptions, ct);
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync<UserDto>(JsonOptions, ct);
        return dto ?? throw new InvalidOperationException("Invalid API response");
    }

    public async Task<UserDto> UpdateAsync(Guid tenantId, string id, string? email, string? displayName, CancellationToken ct)
    {
        var resp = await http.PutAsJsonAsync($"/api/tenants/{tenantId}/users/{id}", new { Email = email, DisplayName = displayName }, JsonOptions, ct);
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync<UserDto>(JsonOptions, ct);
        return dto ?? throw new InvalidOperationException("Invalid API response");
    }

    public async Task<bool> DeleteAsync(Guid tenantId, string id, CancellationToken ct)
    {
        var resp = await http.DeleteAsync($"/api/tenants/{tenantId}/users/{id}", ct);
        if (resp.StatusCode == System.Net.HttpStatusCode.NoContent) return true;
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return false;
        resp.EnsureSuccessStatusCode();
        return true;
    }
}

public sealed class UserDto
{
    public string Id { get; init; } = string.Empty;
    public Guid TenantId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
}

