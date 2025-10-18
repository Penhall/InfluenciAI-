using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using InfluenciAI.Desktop.Services.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InfluenciAI.Desktop.Services.Http;

public class BearerTokenHandler : DelegatingHandler
{
    private readonly IAuthTokenProvider _state;
    private readonly IHttpClientFactory _factory;
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    private record RefreshRequest(string refresh_token);
    private record RefreshResponse(string access_token, string refresh_token);

    public BearerTokenHandler(IAuthTokenProvider state, IHttpClientFactory factory)
    {
        _state = state; _factory = factory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_state.AccessToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _state.AccessToken);
        }

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized)
            return response;

        var retKey = new System.Net.Http.HttpRequestOptionsKey<bool>("_ret");
        if (request.Options.TryGetValue(retKey, out var alreadyRetried) && alreadyRetried)
            return response; // já tentado

        // Não tentar refresh para rotas de auth
        if (request.RequestUri is not null && request.RequestUri.AbsolutePath.StartsWith("/auth", System.StringComparison.OrdinalIgnoreCase))
            return response;

        if (string.IsNullOrWhiteSpace(_state.RefreshToken))
            return response;

        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            // Outro request pode ter atualizado o token enquanto aguardávamos o lock
            if (!string.IsNullOrWhiteSpace(_state.AccessToken))
            {
                // tentar uma única vez com novo token
                using var retry = await CloneRequestAsync(request);
                retry.Options.Set(retKey, true);
                retry.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _state.AccessToken);
                response.Dispose();
                return await base.SendAsync(retry, cancellationToken);
            }

            var client = _factory.CreateClient("auth");
            var rr = new RefreshRequest(_state.RefreshToken!);
            var r = await client.PostAsJsonAsync("/auth/refresh", rr, cancellationToken);
            if (!r.IsSuccessStatusCode)
                return response;
            var payload = await r.Content.ReadFromJsonAsync<RefreshResponse>(cancellationToken: cancellationToken);
            if (payload is null || string.IsNullOrWhiteSpace(payload.access_token) || string.IsNullOrWhiteSpace(payload.refresh_token))
                return response;

            if (_state is IAuthTokenProvider s)
            {
                // Atualiza tokens
                if (s is AuthState concrete)
                    concrete.SetTokens(payload.access_token, payload.refresh_token);
                else
                    s.SetToken(payload.access_token);
            }

            using var retry2 = await CloneRequestAsync(request);
            retry2.Options.Set(retKey, true);
            retry2.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _state.AccessToken);
            response.Dispose();
            return await base.SendAsync(retry2, cancellationToken);
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };
        // Headers
        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        foreach (var prop in request.Options)
            clone.Options.Set(new System.Net.Http.HttpRequestOptionsKey<object?>(prop.Key), prop.Value);

        // Content
        if (request.Content != null)
        {
            var ms = new System.IO.MemoryStream();
            await request.Content.CopyToAsync(ms);
            ms.Position = 0;
            var newContent = new StreamContent(ms);
            foreach (var h in request.Content.Headers)
                newContent.Headers.TryAddWithoutValidation(h.Key, h.Value);
            clone.Content = newContent;
        }
        return clone;
    }
}
