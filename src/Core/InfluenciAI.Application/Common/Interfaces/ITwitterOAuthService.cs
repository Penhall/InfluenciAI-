namespace InfluenciAI.Application.Common.Interfaces;

/// <summary>
/// Service for handling Twitter OAuth 2.0 authentication flow
/// </summary>
public interface ITwitterOAuthService
{
    /// <summary>
    /// Generates the OAuth authorization URL for the user to visit
    /// </summary>
    /// <param name="callbackUrl">The URL to redirect back to after authorization</param>
    /// <returns>Authorization URL with required parameters</returns>
    string GetAuthorizationUrl(string callbackUrl);

    /// <summary>
    /// Exchanges the authorization code for access and refresh tokens
    /// </summary>
    /// <param name="code">Authorization code received from Twitter</param>
    /// <param name="codeVerifier">PKCE code verifier used in the authorization request</param>
    /// <returns>OAuth tokens (access token, refresh token, expires in)</returns>
    Task<TwitterOAuthTokens> ExchangeCodeForTokensAsync(string code, string codeVerifier);

    /// <summary>
    /// Generates a PKCE code verifier and challenge for OAuth 2.0
    /// </summary>
    /// <returns>Code verifier and code challenge</returns>
    (string codeVerifier, string codeChallenge) GeneratePkceChallenge();
}

public record TwitterOAuthTokens(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn
);
