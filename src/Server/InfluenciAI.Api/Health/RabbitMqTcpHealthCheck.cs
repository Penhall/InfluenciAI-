using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace InfluenciAI.Api.Health;

public sealed class RabbitMqTcpHealthCheck(string connectionString) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var uri = new Uri(connectionString);
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5672;
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(host, port);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(3));
            await Task.WhenAny(connectTask, Task.Delay(Timeout.Infinite, cts.Token));
            if (!client.Connected) return HealthCheckResult.Unhealthy($"RabbitMQ TCP not reachable at {host}:{port}");
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ TCP health check failed", ex);
        }
    }
}

