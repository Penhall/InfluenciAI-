using InfluenciAI.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace InfluenciAI.Infrastructure.Services;

/// <summary>
/// Service for interacting with Twitter API using Tweetinvi
/// Implements OAuth 1.0a authentication for Twitter API v1.1
/// </summary>
public class TwitterService : ITwitterService
{
    private readonly IConfiguration _configuration;

    public TwitterService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Gets Twitter profile information using OAuth credentials
    /// </summary>
    public async Task<TwitterProfileInfo> GetProfileInfoAsync(string accessToken)
    {
        try
        {
            // In MVP, we use stored credentials from config
            // In production, accessToken would contain user-specific OAuth tokens
            var client = CreateTwitterClient();

            // Get authenticated user info
            var user = await client.Users.GetAuthenticatedUserAsync();

            if (user == null)
                throw new InvalidOperationException("Failed to retrieve Twitter user profile");

            return new TwitterProfileInfo(
                ProfileId: user.IdStr,
                Username: user.ScreenName,
                DisplayName: user.Name,
                ProfileImageUrl: user.ProfileImageUrl
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get Twitter profile: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Publishes a tweet to Twitter
    /// </summary>
    public async Task<TwitterPublishResult> PublishTweetAsync(string accessToken, string text)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Tweet text cannot be empty", nameof(text));

            if (text.Length > 280)
                throw new ArgumentException("Tweet text cannot exceed 280 characters", nameof(text));

            var client = CreateTwitterClient();

            // Publish tweet
            var tweet = await client.Tweets.PublishTweetAsync(text);

            if (tweet == null)
                throw new InvalidOperationException("Failed to publish tweet - no response from Twitter");

            return new TwitterPublishResult(
                TweetId: tweet.IdStr,
                TweetUrl: tweet.Url
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to publish tweet: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets metrics for a specific tweet
    /// </summary>
    public async Task<TwitterMetrics> GetTweetMetricsAsync(string accessToken, string tweetId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tweetId))
                throw new ArgumentException("Tweet ID cannot be empty", nameof(tweetId));

            var client = CreateTwitterClient();

            // Parse tweet ID to long
            if (!long.TryParse(tweetId, out var tweetIdLong))
                throw new ArgumentException($"Invalid tweet ID format: {tweetId}", nameof(tweetId));

            // Get tweet
            var tweet = await client.Tweets.GetTweetAsync(tweetIdLong);

            if (tweet == null)
                throw new InvalidOperationException($"Tweet not found: {tweetId}");

            // Twitter API v1.1 provides limited metrics
            // For full metrics, would need Twitter API v2 with elevated access
            return new TwitterMetrics(
                Views: 0, // Not available in v1.1
                Likes: tweet.FavoriteCount,
                Retweets: tweet.RetweetCount,
                Replies: 0, // Not directly available in v1.1
                Quotes: tweet.QuoteCount.GetValueOrDefault(0)
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get tweet metrics: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates a TwitterClient with OAuth credentials from configuration
    /// </summary>
    private TwitterClient CreateTwitterClient()
    {
        // Get credentials from configuration
        var consumerKey = _configuration["Twitter:ConsumerKey"];
        var consumerSecret = _configuration["Twitter:ConsumerSecret"];
        var accessToken = _configuration["Twitter:AccessToken"];
        var accessTokenSecret = _configuration["Twitter:AccessTokenSecret"];

        // Validate credentials
        if (string.IsNullOrWhiteSpace(consumerKey))
            throw new InvalidOperationException("Twitter:ConsumerKey not configured");

        if (string.IsNullOrWhiteSpace(consumerSecret))
            throw new InvalidOperationException("Twitter:ConsumerSecret not configured");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new InvalidOperationException("Twitter:AccessToken not configured");

        if (string.IsNullOrWhiteSpace(accessTokenSecret))
            throw new InvalidOperationException("Twitter:AccessTokenSecret not configured");

        // Create credentials
        var credentials = new TwitterCredentials(
            consumerKey,
            consumerSecret,
            accessToken,
            accessTokenSecret
        );

        return new TwitterClient(credentials);
    }
}
