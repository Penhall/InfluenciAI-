using System.Net.Http;
using System.Net.Http.Json;
using InfluenciAI.Domain.Entities;

namespace InfluenciAI.Desktop.Services.Content;

public interface IContentService
{
    Task<List<ContentDto>> GetAllAsync(CancellationToken ct);
    Task<ContentDto> CreateAsync(CreateContentRequest request, CancellationToken ct);
    Task<PublicationDto> PublishAsync(Guid contentId, Guid socialProfileId, CancellationToken ct);
}

public class ContentService(HttpClient http) : IContentService
{
    public async Task<List<ContentDto>> GetAllAsync(CancellationToken ct)
    {
        var response = await http.GetAsync("/api/content", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<ContentDto>>(cancellationToken: ct)
            ?? new List<ContentDto>();
    }

    public async Task<ContentDto> CreateAsync(CreateContentRequest request, CancellationToken ct)
    {
        var response = await http.PostAsJsonAsync("/api/content", request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ContentDto>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Failed to create content");
    }

    public async Task<PublicationDto> PublishAsync(Guid contentId, Guid socialProfileId, CancellationToken ct)
    {
        var request = new { SocialProfileId = socialProfileId };
        var response = await http.PostAsJsonAsync($"/api/content/{contentId}/publish", request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PublicationDto>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Failed to publish content");
    }
}

public record CreateContentRequest(string Title, string Body, ContentType Type = ContentType.Text);

public record ContentDto(
    Guid Id,
    Guid TenantId,
    string UserId,
    string Title,
    string Body,
    ContentType Type,
    ContentStatus Status,
    DateTime? ScheduledFor,
    DateTime? PublishedAt,
    DateTime CreatedAt
);

public record PublicationDto(
    Guid Id,
    Guid ContentId,
    Guid SocialProfileId,
    string ExternalId,
    string ExternalUrl,
    PublicationStatus Status,
    DateTime? PublishedAt,
    string? ErrorMessage
);
