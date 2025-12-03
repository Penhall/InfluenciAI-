using InfluenciAI.Application.Common.Interfaces;
using InfluenciAI.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InfluenciAI.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<InfluenciAI.Infrastructure.Identity.AppUser, IdentityRole, string>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public new DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SocialProfile> SocialProfiles => Set<SocialProfile>();
    public DbSet<Content> Contents => Set<Content>();
    public DbSet<Publication> Publications => Set<Publication>();
    public DbSet<MetricSnapshot> MetricSnapshots => Set<MetricSnapshot>();

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

        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.ToTable("refresh_tokens");
            b.HasKey(x => x.Id);
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.ExpiresAtUtc).IsRequired();
            b.Property(x => x.RevokedAtUtc);
            b.HasIndex(x => x.TokenHash).IsUnique();
        });

        modelBuilder.Entity<SocialProfile>(b =>
        {
            b.ToTable("social_profiles");
            b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.Network).IsRequired();
            b.Property(x => x.ProfileId).HasMaxLength(100).IsRequired();
            b.Property(x => x.Username).HasMaxLength(100).IsRequired();
            b.Property(x => x.DisplayName).HasMaxLength(200);
            b.Property(x => x.ProfileImageUrl).HasMaxLength(500);
            b.Property(x => x.AccessToken).HasMaxLength(500).IsRequired();
            b.Property(x => x.RefreshToken).HasMaxLength(500);
            b.Property(x => x.TokenExpiresAt).IsRequired();
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.ConnectedAt).IsRequired();
            b.HasIndex(x => new { x.TenantId, x.Network, x.ProfileId }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.UserId });
        });

        modelBuilder.Entity<Content>(b =>
        {
            b.ToTable("contents");
            b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.Title).HasMaxLength(500);
            b.Property(x => x.Body).HasMaxLength(5000).IsRequired();
            b.Property(x => x.Type).IsRequired();
            b.Property(x => x.Status).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => new { x.TenantId, x.UserId });
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.ScheduledFor);
        });

        modelBuilder.Entity<Publication>(b =>
        {
            b.ToTable("publications");
            b.HasKey(x => x.Id);
            b.Property(x => x.ContentId).IsRequired();
            b.Property(x => x.SocialProfileId).IsRequired();
            b.Property(x => x.ExternalId).HasMaxLength(100);
            b.Property(x => x.ExternalUrl).HasMaxLength(500);
            b.Property(x => x.Status).IsRequired();
            b.Property(x => x.ErrorMessage).HasMaxLength(1000);
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.ContentId);
            b.HasIndex(x => x.SocialProfileId);
            b.HasIndex(x => new { x.Status, x.PublishedAt });
            b.HasIndex(x => x.ExternalId);
        });

        modelBuilder.Entity<MetricSnapshot>(b =>
        {
            b.ToTable("metric_snapshots");
            b.HasKey(x => x.Id);
            b.Property(x => x.PublicationId).IsRequired();
            b.Property(x => x.CollectedAt).IsRequired();
            b.Property(x => x.Views).IsRequired();
            b.Property(x => x.Likes).IsRequired();
            b.Property(x => x.Retweets).IsRequired();
            b.Property(x => x.Replies).IsRequired();
            b.Property(x => x.EngagementRate).HasPrecision(5, 4).IsRequired();
            b.HasIndex(x => x.PublicationId);
            b.HasIndex(x => new { x.PublicationId, x.CollectedAt });
        });
    }
}

