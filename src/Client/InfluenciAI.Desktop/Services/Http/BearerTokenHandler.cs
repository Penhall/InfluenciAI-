using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using InfluenciAI.Desktop.Services.Auth;

namespace InfluenciAI.Desktop.Services.Http;

public class BearerTokenHandler(IAuthTokenProvider state) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(state.AccessToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", state.AccessToken);
        }
        return base.SendAsync(request, cancellationToken);
    }
}

