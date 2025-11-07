namespace InfluenciAI.Application.Metrics.DTOs;

public record ContentMetricsDto(
    Guid ContentId,
    string Title,
    string Body,
    DateTime? PublishedAt,
    MetricSnapshotDto? LatestMetrics,
    List<MetricSnapshotDto> TimeseriesMetrics
);
