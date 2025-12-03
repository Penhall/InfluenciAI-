using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InfluenciAI.Application.Common.Interfaces;
using InfluenciAI.Application.Metrics.Queries;
using InfluenciAI.Application.Metrics.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InfluenciAI.Application.Metrics.Handlers;

public class GetContentMetricsQueryHandler : IRequestHandler<GetContentMetricsQuery, ContentMetricsDto>
{
    private readonly IApplicationDbContext _context;

    public GetContentMetricsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ContentMetricsDto> Handle(GetContentMetricsQuery request, CancellationToken cancellationToken)
    {
        var content = await _context.Contents
            .Include(c => c.Publications)
            .ThenInclude(p => p.Metrics)
            .FirstOrDefaultAsync(c => c.Id == request.ContentId, cancellationToken);

        if (content == null)
        {
            throw new Exception("Content not found.");
        }

        var latestMetrics = content.Publications
            .SelectMany(p => p.Metrics)
            .OrderByDescending(m => m.CollectedAt)
            .FirstOrDefault();

        var timeseries = content.Publications
            .SelectMany(p => p.Metrics)
            .OrderBy(m => m.CollectedAt)
            .Select(m => new MetricSnapshotDto(
                m.CollectedAt,
                m.Views,
                m.Likes,
                m.Retweets,
                m.Replies,
                m.EngagementRate
            ))
            .ToList();

        return new ContentMetricsDto(
            content.Id,
            content.PublishedAt,
            latestMetrics != null ? new MetricSnapshotDto(
                latestMetrics.CollectedAt,
                latestMetrics.Views,
                latestMetrics.Likes,
                latestMetrics.Retweets,
                latestMetrics.Replies,
                latestMetrics.EngagementRate
            ) : null,
            timeseries
        );
    }
}
