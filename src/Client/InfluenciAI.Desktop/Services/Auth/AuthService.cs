using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace InfluenciAI.Desktop.Services.Auth;

public interface IAuthService
{
    Task<bool> LoginAsync(string email, string password, CancellationToken ct);
}

public class AuthService(HttpClient http, IAuthTokenProvider state) : IAuthService
{
    private record LoginRequest(string Email, string Password);
    private record LoginResponse(string access_token);

    public async Task<bool> LoginAsync(string email, string password, CancellationToken ct)
    {
        var resp = await http.PostAsJsonAsync("/auth/login", new LoginRequest(email, password), ct);
        if (!resp.IsSuccessStatusCode) return false;
        var payload = await resp.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
        if (payload is null || string.IsNullOrWhiteSpace(payload.access_token)) return false;
        state.SetToken(payload.access_token);
        return true;
    }
}

