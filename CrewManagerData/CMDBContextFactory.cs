using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CrewManagerData;

public class CMDBContextFactory : IDesignTimeDbContextFactory<CMDBContext>
{
    public CMDBContext CreateDbContext(string[] args)
    {
        // Try multiple paths to find appsettings.json
        var basePaths = new[]
        {
            Directory.GetCurrentDirectory(),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "CrewManagerAPI"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "CrewManagerAPI")
        };

        IConfigurationRoot? configuration = null;

        foreach (var basePath in basePaths)
        {
            try
            {
                var configPath = Path.Combine(basePath, "appsettings.json");
                if (File.Exists(configPath))
                {
                    configuration = new ConfigurationBuilder()
                        .SetBasePath(basePath)
                        .AddJsonFile("appsettings.json", optional: false)
                        .AddJsonFile("appsettings.Development.json", optional: true)
                        .Build();
                    break;
                }
            }
            catch
            {
                // Continue to next path
            }
        }

        if (configuration == null)
        {
            throw new InvalidOperationException("Could not find appsettings.json in any of the expected locations.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<CMDBContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection string not found in configuration.");
        }

        optionsBuilder.UseNpgsql(connectionString);

        return new CMDBContext(optionsBuilder.Options);
    }
}
