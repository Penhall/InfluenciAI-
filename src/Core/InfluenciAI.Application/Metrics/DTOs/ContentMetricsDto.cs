namespace InfluenciAI.Application.Metrics.DTOs;

public record ContentMetricsDto(
    Guid ContentId,
    DateTime? PublishedAt,
    MetricSnapshotDto? Latest,
    List<MetricSnapshotDto> Timeseries
);
