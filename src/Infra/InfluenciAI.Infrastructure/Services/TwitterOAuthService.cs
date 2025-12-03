using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InfluenciAI.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace InfluenciAI.Infrastructure.Services;

/// <summary>
/// Implementation of Twitter OAuth 2.0 with PKCE flow
/// </summary>
public class TwitterOAuthService : ITwitterOAuthService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    private const string AuthorizationEndpoint = "https://twitter.com/i/oauth2/authorize";
    private const string TokenEndpoint = "https://api.twitter.com/2/oauth2/token";

    public TwitterOAuthService(IConfiguration config, HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;
    }

    public string GetAuthorizationUrl(string callbackUrl)
    {
        var clientId = _config["Twitter:ClientId"] ?? throw new InvalidOperationException("Twitter:ClientId not configured");

        // Generate PKCE challenge
        var (codeVerifier, codeChallenge) = GeneratePkceChallenge();

        // Store code verifier temporarily (in production, use distributed cache or session)
        // For now, we'll pass it as state parameter (NOT recommended for production)

        var queryParams = new Dictionary<string, string>
        {
            ["response_type"] = "code",
            ["client_id"] = clientId,
            ["redirect_uri"] = callbackUrl,
            ["scope"] = "tweet.read tweet.write users.read offline.access",
            ["state"] = GenerateState(),
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = "S256"
        };

        var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{AuthorizationEndpoint}?{queryString}";
    }

    public async Task<TwitterOAuthTokens> ExchangeCodeForTokensAsync(string code, string codeVerifier)
    {
        var clientId = _config["Twitter:ClientId"] ?? throw new InvalidOperationException("Twitter:ClientId not configured");
        var clientSecret = _config["Twitter:ClientSecret"];
        var redirectUri = _config["Twitter:RedirectUri"] ?? "http://localhost:5228/auth/twitter/callback";

        var requestBody = new Dictionary<string, string>
        {
            ["code"] = code,
            ["grant_type"] = "authorization_code",
            ["client_id"] = clientId,
            ["redirect_uri"] = redirectUri,
            ["code_verifier"] = codeVerifier
        };

        // Add client secret if using confidential client
        if (!string.IsNullOrEmpty(clientSecret))
        {
            requestBody["client_secret"] = clientSecret;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(requestBody)
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TwitterTokenResponse>(responseContent);

        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.access_token))
        {
            throw new InvalidOperationException("Failed to obtain access token from Twitter");
        }

        return new TwitterOAuthTokens(
            tokenResponse.access_token,
            tokenResponse.refresh_token ?? string.Empty,
            tokenResponse.expires_in
        );
    }

    public (string codeVerifier, string codeChallenge) GeneratePkceChallenge()
    {
        // Generate code verifier (43-128 characters)
        var codeVerifier = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        // Generate code challenge (SHA256 hash of verifier)
        var codeChallenge = Convert.ToBase64String(SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier)))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        return (codeVerifier, codeChallenge);
    }

    private static string GenerateState()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private record TwitterTokenResponse(
        string access_token,
        string? refresh_token,
        int expires_in,
        string token_type
    );
}
