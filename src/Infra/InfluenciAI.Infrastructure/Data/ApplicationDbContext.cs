using InfluenciAI.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InfluenciAI.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<InfluenciAI.Infrastructure.Identity.AppUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public new DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(b =>
        {
            b.ToTable("tenants");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("users");
            b.HasKey(x => x.Id);
            b.Property(x => x.Email).HasMaxLength(320).IsRequired();
            b.Property(x => x.DisplayName).HasMaxLength(200);
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
        });
    }
}

