namespace InfluenciAI.Application.Tenants;

public sealed class TenantDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
}

