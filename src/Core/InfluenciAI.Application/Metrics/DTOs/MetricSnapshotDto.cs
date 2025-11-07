namespace InfluenciAI.Application.Metrics.DTOs;

public record MetricSnapshotDto(
    Guid Id,
    Guid PublicationId,
    DateTime CollectedAt,
    int Views,
    int Likes,
    int Shares,
    int Comments,
    decimal EngagementRate,
    int Impressions,
    int Clicks
);
