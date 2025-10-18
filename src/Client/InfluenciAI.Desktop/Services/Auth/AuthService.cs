using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace InfluenciAI.Desktop.Services.Auth;

public interface IAuthService
{
    Task<bool> LoginAsync(string email, string password, CancellationToken ct);
    Task<bool> RefreshAsync(CancellationToken ct);
    Task LogoutAsync(CancellationToken ct);
    Task LogoutAllAsync(CancellationToken ct);
}

public class AuthService(HttpClient http, IAuthTokenProvider state) : IAuthService
{
    private record LoginRequest(string Email, string Password);
    private record LoginResponse(string access_token, string? refresh_token);
    private record RefreshRequest(string refresh_token);
    private record LogoutRequest(string refresh_token);

    public async Task<bool> LoginAsync(string email, string password, CancellationToken ct)
    {
        var resp = await http.PostAsJsonAsync("/auth/login", new LoginRequest(email, password), ct);
        if (!resp.IsSuccessStatusCode) return false;
        var payload = await resp.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
        if (payload is null || string.IsNullOrWhiteSpace(payload.access_token)) return false;
        if (!string.IsNullOrWhiteSpace(payload.refresh_token))
            state.SetTokens(payload.access_token, payload.refresh_token);
        else
            state.SetToken(payload.access_token);
        return true;
    }

    public async Task<bool> RefreshAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(state.RefreshToken)) return false;
        var resp = await http.PostAsJsonAsync("/auth/refresh", new RefreshRequest(state.RefreshToken!), ct);
        if (!resp.IsSuccessStatusCode) return false;
        var payload = await resp.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
        if (payload is null || string.IsNullOrWhiteSpace(payload.access_token) || string.IsNullOrWhiteSpace(payload.refresh_token)) return false;
        state.SetTokens(payload.access_token, payload.refresh_token!);
        return true;
    }

    public async Task LogoutAsync(CancellationToken ct)
    {
        var token = state.RefreshToken ?? string.Empty;
        try
        {
            await http.PostAsJsonAsync("/auth/logout", new LogoutRequest(token), ct);
        }
        finally
        {
            state.SetTokens(string.Empty, string.Empty);
        }
    }

    public async Task LogoutAllAsync(CancellationToken ct)
    {
        try
        {
            await http.PostAsync("/auth/logout/all", content: null, ct);
        }
        finally
        {
            state.SetTokens(string.Empty, string.Empty);
        }
    }
}
