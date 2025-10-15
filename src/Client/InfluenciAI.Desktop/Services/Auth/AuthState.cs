namespace InfluenciAI.Desktop.Services.Auth;

public interface IAuthTokenProvider
{
    string? AccessToken { get; }
    void SetToken(string token);
}

public class AuthState : IAuthTokenProvider
{
    public string? AccessToken { get; private set; }
    public void SetToken(string token) => AccessToken = token;
}

