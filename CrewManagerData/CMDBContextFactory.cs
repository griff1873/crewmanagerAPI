using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using CrewManagerData;
using System.IO;

public class CMDBContextFactory : IDesignTimeDbContextFactory<CMDBContext>
{
    public CMDBContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // adjust as needed
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<CMDBContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseNpgsql(connectionString); // or UseNpgsql, UseSqlite, etc.

        return new CMDBContext(optionsBuilder.Options);
    }
}
