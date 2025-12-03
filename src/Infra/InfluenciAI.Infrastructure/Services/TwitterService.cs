using InfluenciAI.Application.Common.Exceptions;
using InfluenciAI.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Tweetinvi;
using Tweetinvi.Exceptions;
using Tweetinvi.Models;

namespace InfluenciAI.Infrastructure.Services;

public class TwitterService : ITwitterService
{
    private readonly IConfiguration _config;

    public TwitterService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<TwitterUserProfile> GetUserProfileAsync(string accessToken)
    {
        try
        {
            var client = new TwitterClient(_config["Twitter:ConsumerKey"], _config["Twitter:ConsumerSecret"], accessToken, _config["Twitter:AccessTokenSecret"]);
            var user = await client.Users.GetAuthenticatedUserAsync();

            return new TwitterUserProfile(
                user.IdStr,
                user.ScreenName,
                user.Name,
                user.ProfileImageUrl
            );
        }
        catch (TwitterException ex) when (ex.StatusCode == 429)
        {
            throw new RateLimitExceededException("Twitter API rate limit exceeded.", ex);
        }
        catch (TwitterException ex) when (ex.StatusCode == 401)
        {
            throw new TokenExpirationException("Twitter access token has expired.", ex);
        }
    }

    public async Task<TweetResult> PublishTweetAsync(string accessToken, string text)
    {
        try
        {
            var client = new TwitterClient(_config["Twitter:ConsumerKey"], _config["Twitter:ConsumerSecret"], accessToken, _config["Twitter:AccessTokenSecret"]);
            var tweet = await client.Tweets.PublishTweetAsync(text);

            return new TweetResult(
                tweet.IdStr,
                tweet.Url
            );
        }
        catch (TwitterException ex) when (ex.StatusCode == 429)
        {
            throw new RateLimitExceededException("Twitter API rate limit exceeded.", ex);
        }
        catch (TwitterException ex) when (ex.StatusCode == 401)
        {
            throw new TokenExpirationException("Twitter access token has expired.", ex);
        }
    }

    public async Task<TweetMetrics> GetTweetMetricsAsync(string accessToken, string tweetId)
    {
        try
        {
            var client = new TwitterClient(_config["Twitter:ConsumerKey"], _config["Twitter:ConsumerSecret"], accessToken, _config["Twitter:AccessTokenSecret"]);
            var tweet = await client.Tweets.GetTweetAsync(long.Parse(tweetId));

            return new TweetMetrics(
                tweet.RetweetCount + (tweet.QuoteCount ?? 0), // views approximation
                tweet.FavoriteCount,
                tweet.RetweetCount,
                0 // replies não disponível diretamente
            );
        }
        catch (TwitterException ex) when (ex.StatusCode == 429)
        {
            throw new RateLimitExceededException("Twitter API rate limit exceeded.", ex);
        }
        catch (TwitterException ex) when (ex.StatusCode == 401)
        {
            throw new TokenExpirationException("Twitter access token has expired.", ex);
        }
    }
}
