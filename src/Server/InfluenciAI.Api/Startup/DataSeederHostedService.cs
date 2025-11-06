using InfluenciAI.Application.Tenants;
using InfluenciAI.Infrastructure.Data;
using InfluenciAI.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InfluenciAI.Api.Startup;

public class DataSeederHostedService(IServiceScopeFactory scopeFactory, ILogger<DataSeederHostedService> logger, IHostEnvironment env) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!env.IsDevelopment()) return;

        using var scope = scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Only run migrations if using a relational database provider
        var isInMemory = ctx.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
        if (!isInMemory)
        {
            await ctx.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            logger.LogWarning("Using InMemory database - skipping migrations. Configure User Secrets to use PostgreSQL.");
            await ctx.Database.EnsureCreatedAsync(cancellationToken);
        }

        var tenants = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var tenantName = config["Seed:Tenant:Name"] ?? "Default";
        var adminEmail = config["Seed:Admin:Email"] ?? "admin@local";
        var adminPassword = config["Seed:Admin:Password"] ?? "Admin!234";

        var all = await tenants.ListAsync(cancellationToken);
        var tenant = all.FirstOrDefault(t => t.Name == tenantName);
        tenant ??= await tenants.AddAsync(new InfluenciAI.Domain.Entities.Tenant { Name = tenantName }, cancellationToken);
        await tenants.SaveChangesAsync(cancellationToken);

        if (!await roleManager.RoleExistsAsync("admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("admin"));
        }

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                TenantId = tenant.Id,
                DisplayName = "Administrator",
                EmailConfirmed = true
            };
            var res = await userManager.CreateAsync(admin, adminPassword);
            if (!res.Succeeded)
            {
                logger.LogWarning("Failed to create admin user: {Errors}", string.Join(", ", res.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(admin!, "admin"))
        {
            await userManager.AddToRoleAsync(admin!, "admin");
        }

        logger.LogInformation("Seed completed. Tenant={Tenant}, Admin={Admin}", tenant.Name, adminEmail);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
