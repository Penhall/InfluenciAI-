using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace InfluenciAI.Api.Cors;

public class TenantCorsPolicyProvider(IConfiguration configuration) : ICorsPolicyProvider
{
    public Task<CorsPolicy?> GetPolicyAsync(HttpContext context, string? policyName)
    {
        var allowAll = configuration.GetValue("Cors:AllowAllInDev", false);
        if (allowAll && context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            var dev = new CorsPolicyBuilder().AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().Build();
            return Task.FromResult<CorsPolicy?>(dev);
        }

        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out StringValues tenantVals))
        {
            var tenantId = tenantVals.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                var section = configuration.GetSection($"Cors:Tenants:{tenantId}:AllowedOrigins");
                var tenantOrigins = section.Get<string[]>() ?? Array.Empty<string>();
                if (tenantOrigins.Length > 0)
                    origins = tenantOrigins;
            }
        }

        var policy = new CorsPolicyBuilder()
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .Build();

        return Task.FromResult<CorsPolicy?>(policy);
    }
}

