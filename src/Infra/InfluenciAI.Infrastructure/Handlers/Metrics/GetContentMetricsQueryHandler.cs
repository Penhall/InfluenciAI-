using InfluenciAI.Application.Common.Interfaces;
using InfluenciAI.Application.Metrics.DTOs;
using InfluenciAI.Application.Metrics.Queries;
using InfluenciAI.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InfluenciAI.Infrastructure.Handlers.Metrics;

public class GetContentMetricsQueryHandler : IRequestHandler<GetContentMetricsQuery, ContentMetricsDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetContentMetricsQueryHandler(
        ApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ContentMetricsDto> Handle(GetContentMetricsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var tenantId = _currentUserService.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        // Get content with publications
        var content = await _context.Contents
            .Include(c => c.Publications)
            .ThenInclude(p => p.MetricSnapshots)
            .FirstOrDefaultAsync(c =>
                c.Id == request.ContentId &&
                c.TenantId == tenantId &&
                c.UserId == userId,
                cancellationToken);

        if (content == null)
            throw new InvalidOperationException("Content not found");

        // Get all metric snapshots for all publications of this content
        var allSnapshots = content.Publications
            .SelectMany(p => p.MetricSnapshots)
            .OrderByDescending(m => m.CollectedAt)
            .ToList();

        // Get latest snapshot
        var latestSnapshot = allSnapshots.FirstOrDefault();

        return new ContentMetricsDto(
            ContentId: content.Id,
            Title: content.Title,
            Body: content.Body,
            PublishedAt: content.PublishedAt,
            LatestMetrics: latestSnapshot != null ? MapToDto(latestSnapshot) : null,
            TimeseriesMetrics: allSnapshots.Select(MapToDto).ToList()
        );
    }

    private static MetricSnapshotDto MapToDto(Domain.Entities.MetricSnapshot snapshot)
    {
        return new MetricSnapshotDto(
            Id: snapshot.Id,
            PublicationId: snapshot.PublicationId,
            CollectedAt: snapshot.CollectedAt,
            Views: snapshot.Views,
            Likes: snapshot.Likes,
            Shares: snapshot.Shares,
            Comments: snapshot.Comments,
            EngagementRate: snapshot.EngagementRate,
            Impressions: snapshot.Impressions,
            Clicks: snapshot.Clicks
        );
    }
}
