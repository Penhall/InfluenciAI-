using InfluenciAI.Application.Tenants;
using InfluenciAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InfluenciAI.Infrastructure.Data;

public sealed class TenantRepository(ApplicationDbContext db) : ITenantRepository
{
    public async Task<Tenant> AddAsync(Tenant tenant, CancellationToken ct)
    {
        await db.Tenants.AddAsync(tenant, ct);
        return tenant;
    }

    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.Tenants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Tenant?> GetByIdForUpdateAsync(Guid id, CancellationToken ct)
        => db.Tenants.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<Tenant>> ListAsync(CancellationToken ct)
        => db.Tenants.AsNoTracking().ToListAsync(ct);

    public void Remove(Tenant tenant)
        => db.Tenants.Remove(tenant);

    public Task<int> SaveChangesAsync(CancellationToken ct)
        => db.SaveChangesAsync(ct);
}
