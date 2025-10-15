using InfluenciAI.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InfluenciAI.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var cs = Environment.GetEnvironmentVariable("INFLUENCIAI_DB")
                 ?? "Host=localhost;Port=5432;Database=influenciai;Username=postgres;Password=postgres";
        optionsBuilder.UseNpgsql(cs);
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

