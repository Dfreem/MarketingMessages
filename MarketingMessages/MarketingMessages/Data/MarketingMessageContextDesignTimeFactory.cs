using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

using System.IO;

namespace MarketingMessages.Data;

public class MarketingMessagesContextFactory : IDesignTimeDbContextFactory<MarketingMessagesContext>
{
    public MarketingMessagesContext CreateDbContext(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("MarketingMessagesConnection");

        // Configure DbContextOptions
        var optionsBuilder = new DbContextOptionsBuilder<MarketingMessagesContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new MarketingMessagesContext(optionsBuilder.Options);
    }
}