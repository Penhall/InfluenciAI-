using InfluenciAI.Domain.Entities;

namespace InfluenciAI.Application.Tenants;

public interface ITenantRepository
{
    Task<Tenant> AddAsync(Tenant tenant, CancellationToken ct);
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Tenant?> GetByIdForUpdateAsync(Guid id, CancellationToken ct);
    Task<List<Tenant>> ListAsync(CancellationToken ct);
    void Remove(Tenant tenant);
    Task<int> SaveChangesAsync(CancellationToken ct);
}
