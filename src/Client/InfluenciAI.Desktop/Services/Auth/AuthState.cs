namespace InfluenciAI.Desktop.Services.Auth;

public interface IAuthTokenProvider
{
    string? AccessToken { get; }
    string? RefreshToken { get; }
    void SetToken(string token);
    void SetTokens(string accessToken, string refreshToken);
}

public class AuthState : IAuthTokenProvider
{
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public void SetToken(string token) => AccessToken = token;
    public void SetTokens(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }
}
