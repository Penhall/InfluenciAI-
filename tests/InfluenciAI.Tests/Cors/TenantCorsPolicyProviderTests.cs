using System.Collections.Generic;
using FluentAssertions;
using InfluenciAI.Api.Cors;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace InfluenciAI.Tests.Cors;

public class TenantCorsPolicyProviderTests
{
    private static IConfiguration BuildConfig(Dictionary<string, string?> values)
        => new ConfigurationBuilder().AddInMemoryCollection(values!).Build();

    [Fact]
    public async Task Uses_global_allowed_origins_when_no_tenant_header()
    {
        var cfg = BuildConfig(new()
        {
            ["Cors:AllowedOrigins:0"] = "http://localhost:3000",
            ["Cors:AllowedOrigins:1"] = "http://localhost:5173"
        });
        var provider = new TenantCorsPolicyProvider(cfg);
        var ctx = new DefaultHttpContext();

        var policy = await provider.GetPolicyAsync(ctx, null);
        policy.Should().NotBeNull();
        policy!.Origins.Should().BeEquivalentTo("http://localhost:3000", "http://localhost:5173");
    }

    [Fact]
    public async Task Uses_tenant_override_when_header_present()
    {
        var cfg = BuildConfig(new()
        {
            ["Cors:AllowedOrigins:0"] = "http://global-a:3000",
            ["Cors:Tenants:tenant-123:AllowedOrigins:0"] = "http://tenant-123-a:3000",
            ["Cors:Tenants:tenant-123:AllowedOrigins:1"] = "http://tenant-123-b:3000",
        });
        var provider = new TenantCorsPolicyProvider(cfg);
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["X-Tenant-Id"] = "tenant-123";

        var policy = await provider.GetPolicyAsync(ctx, null);
        policy.Should().NotBeNull();
        policy!.Origins.Should().BeEquivalentTo("http://tenant-123-a:3000", "http://tenant-123-b:3000");
    }
}

