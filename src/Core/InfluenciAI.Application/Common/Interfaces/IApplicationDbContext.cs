using System.Threading;
using System.Threading.Tasks;
using InfluenciAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InfluenciAI.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<SocialProfile> SocialProfiles { get; }
    DbSet<InfluenciAI.Domain.Entities.Content> Contents { get; }
    DbSet<Publication> Publications { get; }
    DbSet<MetricSnapshot> MetricSnapshots { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
