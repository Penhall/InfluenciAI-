namespace InfluenciAI.Application.Common.Interfaces;

public interface ITwitterService
{
    /// <summary>
    /// Gets Twitter profile information from access token
    /// </summary>
    Task<TwitterProfileInfo> GetProfileInfoAsync(string accessToken);

    /// <summary>
    /// Publishes a tweet to Twitter
    /// </summary>
    Task<TwitterPublishResult> PublishTweetAsync(string accessToken, string text);
}

public record TwitterProfileInfo(
    string ProfileId,
    string Username,
    string DisplayName,
    string ProfileImageUrl
);

public record TwitterPublishResult(
    string TweetId,
    string TweetUrl
);
