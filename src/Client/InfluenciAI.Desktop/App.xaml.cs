using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using InfluenciAI.Desktop.Services.Auth;
using InfluenciAI.Desktop.Services.Http;

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

        Services = services.BuildServiceProvider();
    }
}

