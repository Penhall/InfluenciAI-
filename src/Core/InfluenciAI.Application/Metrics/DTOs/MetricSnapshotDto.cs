namespace InfluenciAI.Application.Metrics.DTOs;

public record MetricSnapshotDto(
    DateTime CollectedAt,
    int Views,
    int Likes,
    int Retweets,
    int Replies,
    decimal EngagementRate
);
