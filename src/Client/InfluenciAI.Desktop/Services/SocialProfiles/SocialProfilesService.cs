using System.Net.Http;
using System.Net.Http.Json;
using InfluenciAI.Domain.Entities;

namespace InfluenciAI.Desktop.Services.SocialProfiles;

public interface ISocialProfilesService
{
    Task<List<SocialProfileDto>> GetAllAsync(CancellationToken ct);
    Task<string> GetTwitterAuthUrlAsync(CancellationToken ct);
}

public class SocialProfilesService(HttpClient http) : ISocialProfilesService
{
    public async Task<List<SocialProfileDto>> GetAllAsync(CancellationToken ct)
    {
        var response = await http.GetAsync("/api/social-profiles", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<SocialProfileDto>>(cancellationToken: ct)
            ?? new List<SocialProfileDto>();
    }

    public async Task<string> GetTwitterAuthUrlAsync(CancellationToken ct)
    {
        // Returns the full authorization URL
        var baseUrl = http.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost:5228";
        return $"{baseUrl}/auth/twitter/authorize";
    }
}

public record SocialProfileDto(
    Guid Id,
    Guid TenantId,
    string UserId,
    SocialNetwork Network,
    string ProfileId,
    string Username,
    string DisplayName,
    string ProfileImageUrl,
    bool IsActive,
    DateTime ConnectedAt,
    DateTime? LastSyncAt,
    DateTime TokenExpiresAt
);
