using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RealEstateWeb.Data;

// Design-time tools can read the database configuration without starting the web app.
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var directory = Directory.GetCurrentDirectory();
        if (!File.Exists(Path.Combine(directory, "appsettings.json")))
        {
            directory = AppContext.BaseDirectory;
        }

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
        var configuration = new ConfigurationBuilder()
            .SetBasePath(directory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connection = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Thiếu ConnectionStrings:DefaultConnection trong cấu hình.");
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connection)
            .Options;

        return new ApplicationDbContext(options);
    }
}
