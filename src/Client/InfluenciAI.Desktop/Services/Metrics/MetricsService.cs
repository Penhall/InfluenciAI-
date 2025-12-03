using System.Net.Http;
using System.Net.Http.Json;

namespace InfluenciAI.Desktop.Services.Metrics;

public interface IMetricsService
{
    Task<ContentMetricsDto?> GetContentMetricsAsync(Guid contentId, CancellationToken ct);
}

public class MetricsService(HttpClient http) : IMetricsService
{
    public async Task<ContentMetricsDto?> GetContentMetricsAsync(Guid contentId, CancellationToken ct)
    {
        var response = await http.GetAsync($"/api/content/{contentId}/metrics", ct);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<ContentMetricsDto>(cancellationToken: ct);
    }
}

public record ContentMetricsDto(
    Guid ContentId,
    DateTime? PublishedAt,
    MetricSnapshotDto? Latest,
    List<MetricSnapshotDto> Timeseries
);

public record MetricSnapshotDto(
    DateTime CollectedAt,
    int Views,
    int Likes,
    int Retweets,
    int Replies,
    decimal EngagementRate
);
