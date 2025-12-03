using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using InfluenciAI.Desktop.Services.Auth;
using InfluenciAI.Desktop.Services.Http;
using InfluenciAI.Desktop.Services.Tenants;
using InfluenciAI.Desktop.Services.Users;
using InfluenciAI.Desktop.Services.SocialProfiles;
using InfluenciAI.Desktop.Services.Content;
using InfluenciAI.Desktop.Services.Metrics;

namespace InfluenciAI.Desktop;

public partial class App : System.Windows.Application
{
    public IServiceProvider Services { get; private set; } = default!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddSingleton<IAuthTokenProvider, AuthState>();
        services.AddTransient<BearerTokenHandler>();
        services.AddHttpClient<IAuthService, AuthService>((sp, client) =>
        {
            var baseUrl = sp.GetRequiredService<IConfiguration>()["Api:BaseUrl"] ?? "http://localhost:5000";
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<BearerTokenHandler>();

        services.AddHttpClient<ITenantsService, TenantsService>((sp, client) =>
        {
            var baseUrl = sp.GetRequiredService<IConfiguration>()["Api:BaseUrl"] ?? "http://localhost:5000";
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<BearerTokenHandler>();

        services.AddHttpClient<IUsersService, UsersService>((sp, client) =>
        {
            var baseUrl = sp.GetRequiredService<IConfiguration>()["Api:BaseUrl"] ?? "http://localhost:5000";
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<BearerTokenHandler>();

        // Social Profiles Service
        services.AddHttpClient<ISocialProfilesService, SocialProfilesService>((sp, client) =>
        {
            var baseUrl = sp.GetRequiredService<IConfiguration>()["Api:BaseUrl"] ?? "http://localhost:5000";
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<BearerTokenHandler>();

        // Content Service
        services.AddHttpClient<IContentService, ContentService>((sp, client) =>
        {
            var baseUrl = sp.GetRequiredService<IConfiguration>()["Api:BaseUrl"] ?? "http://localhost:5000";
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<BearerTokenHandler>();

        // Metrics Service
        services.AddHttpClient<IMetricsService, MetricsService>((sp, client) =>
        {
            var baseUrl = sp.GetRequiredService<IConfiguration>()["Api:BaseUrl"] ?? "http://localhost:5000";
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<BearerTokenHandler>();

        // Named client sem handler para fluxos de auth (evita recursão no refresh)
        services.AddHttpClient("auth", (sp, client) =>
        {
            var baseUrl = sp.GetRequiredService<IConfiguration>()["Api:BaseUrl"] ?? "http://localhost:5000";
            client.BaseAddress = new Uri(baseUrl);
        });

        Services = services.BuildServiceProvider();
    }
}

