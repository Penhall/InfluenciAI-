namespace InfluenciAI.Application.Common.Interfaces;

public interface ITwitterService
{
    Task<TwitterUserProfile> GetUserProfileAsync(string accessToken);
    Task<TweetResult> PublishTweetAsync(string accessToken, string text);
    Task<TweetMetrics> GetTweetMetricsAsync(string accessToken, string tweetId);
}

public record TwitterUserProfile(string Id, string Username, string Name, string ProfileImageUrl);
public record TweetResult(string Id, string Url);
public record TweetMetrics(int Views, int Likes, int Retweets, int Replies);
