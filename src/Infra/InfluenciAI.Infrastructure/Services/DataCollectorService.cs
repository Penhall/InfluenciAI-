using InfluenciAI.Application.Common.Interfaces;
using InfluenciAI.Domain.Entities;
using InfluenciAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InfluenciAI.Infrastructure.Services;

/// <summary>
/// Background service that collects metrics from social media posts
/// Runs periodically to update engagement metrics
/// </summary>
public class DataCollectorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataCollectorService> _logger;
    private readonly TimeSpan _collectionInterval = TimeSpan.FromMinutes(5);

    public DataCollectorService(
        IServiceProvider serviceProvider,
        ILogger<DataCollectorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DataCollectorService started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CollectMetricsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error collecting metrics");
                }

                // Wait before next collection cycle
                await Task.Delay(_collectionInterval, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the service is stopping
            _logger.LogInformation("DataCollectorService stopping...");
        }

        _logger.LogInformation("DataCollectorService stopped");
    }

    private async Task CollectMetricsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var twitterService = scope.ServiceProvider.GetRequiredService<ITwitterService>();

        // Get recently published posts (last 24 hours)
        var cutoffTime = DateTime.UtcNow.AddHours(-24);
        var recentPublications = await context.Publications
            .Include(p => p.SocialProfile)
            .Where(p =>
                p.Status == PublicationStatus.Published &&
                p.PublishedAt.HasValue &&
                p.PublishedAt.Value >= cutoffTime)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} recent publications to collect metrics for", recentPublications.Count);

        foreach (var publication in recentPublications)
        {
            try
            {
                // Determine if we should collect based on time since publish
                if (!ShouldCollectMetrics(publication))
                    continue;

                // Only collect for Twitter for now
                if (publication.SocialProfile?.Network != SocialNetwork.Twitter)
                    continue;

                _logger.LogInformation("Collecting metrics for publication {PublicationId}, tweet {TweetId}",
                    publication.Id, publication.ExternalId);

                // Collect metrics from Twitter
                var metrics = await twitterService.GetTweetMetricsAsync(
                    publication.SocialProfile.AccessToken,
                    publication.ExternalId
                );

                // Calculate engagement rate
                var totalEngagement = metrics.Likes + metrics.Retweets + metrics.Replies + metrics.Quotes;
                var engagementRate = metrics.Views > 0
                    ? (decimal)totalEngagement / metrics.Views
                    : 0m;

                // Create snapshot
                var snapshot = new MetricSnapshot
                {
                    PublicationId = publication.Id,
                    CollectedAt = DateTime.UtcNow,
                    Views = metrics.Views,
                    Likes = metrics.Likes,
                    Shares = metrics.Retweets,
                    Comments = metrics.Replies,
                    EngagementRate = engagementRate,
                    Impressions = metrics.Views, // Using views as impressions for now
                    Clicks = 0 // Not available from Twitter API v1.1
                };

                context.MetricSnapshots.Add(snapshot);
                await context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Saved metrics snapshot for publication {PublicationId}: Likes={Likes}, Retweets={Retweets}",
                    publication.Id, metrics.Likes, metrics.Retweets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting metrics for publication {PublicationId}", publication.Id);
            }
        }
    }

    /// <summary>
    /// Determines if we should collect metrics based on time since publish
    /// More frequent collection for recent posts, less frequent for older posts
    /// </summary>
    private bool ShouldCollectMetrics(Publication publication)
    {
        if (!publication.PublishedAt.HasValue)
            return false;

        var timeSincePublish = DateTime.UtcNow - publication.PublishedAt.Value;

        // First 2 hours: collect every time (every 5 minutes)
        if (timeSincePublish.TotalHours <= 2)
            return true;

        // 2-6 hours: collect every 30 minutes
        if (timeSincePublish.TotalHours <= 6)
            return DateTime.UtcNow.Minute % 30 == 0;

        // 6-24 hours: collect every hour
        if (timeSincePublish.TotalHours <= 24)
            return DateTime.UtcNow.Minute == 0;

        // Older than 24 hours: don't collect (handled by cutoff in query)
        return false;
    }
}
