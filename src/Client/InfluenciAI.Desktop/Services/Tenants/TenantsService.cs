using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace InfluenciAI.Desktop.Services.Tenants;

public interface ITenantsService
{
    Task<List<TenantDto>> GetAllAsync(CancellationToken ct);
    Task<TenantDto> CreateAsync(string name, CancellationToken ct);
    Task<TenantDto> UpdateAsync(Guid id, string name, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}

public sealed class TenantsService(HttpClient http) : ITenantsService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<List<TenantDto>> GetAllAsync(CancellationToken ct)
        => await http.GetFromJsonAsync<List<TenantDto>>("/api/tenants", JsonOptions, ct) ?? new List<TenantDto>();

    public async Task<TenantDto> CreateAsync(string name, CancellationToken ct)
    {
        var resp = await http.PostAsJsonAsync("/api/tenants", new { Name = name }, JsonOptions, ct);
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync<TenantDto>(JsonOptions, ct);
        return dto ?? throw new InvalidOperationException("Invalid API response");
    }

    public async Task<TenantDto> UpdateAsync(Guid id, string name, CancellationToken ct)
    {
        var resp = await http.PutAsJsonAsync($"/api/tenants/{id}", new { Name = name }, JsonOptions, ct);
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync<TenantDto>(JsonOptions, ct);
        return dto ?? throw new InvalidOperationException("Invalid API response");
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var resp = await http.DeleteAsync($"/api/tenants/{id}", ct);
        if (resp.StatusCode == System.Net.HttpStatusCode.NoContent) return true;
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return false;
        resp.EnsureSuccessStatusCode();
        return true;
    }
}

public sealed class TenantDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
}

