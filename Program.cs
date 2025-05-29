using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker.Configuration;
using Microsoft.EntityFrameworkCore;
using MyIsolatedFuncApp;             // adjust to your root namespace
using MyIsolatedFuncApp.Data;        // namespace for MyDbContext

var host = Host.CreateDefaultBuilder(args)
    // 1) Bring in local.settings.json so FUNCTIONS values are available
    .ConfigureAppConfiguration((ctx, config) =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)  // for local dev :contentReference[oaicite:0]{index=0}
              .AddEnvironmentVariables();                                                // for Azure app settings
    })

    // 2) Wire up attribute-based Functions discovery
    .ConfigureFunctionsWebApplication()

    // 3) Register your EF Core DbContext factory
    .ConfigureServices((context, services) =>
    {
        // Make sure the key matches what you put in local.settings.json under "Values"
        var conn = context.Configuration["PostgresConnection "];
        if (string.IsNullOrEmpty(conn))
            throw new InvalidOperationException("PostgresConnection  is not configured!");

        services.AddDbContextFactory<MyDbContext>(opts =>
            opts.UseNpgsql(conn)
        );
    })
    .Build();

// ─── Auto-apply EF migrations ───────────────────────────────────────────
using var scope = host.Services.CreateScope();
var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MyDbContext>>();
await using var db = factory.CreateDbContext();
db.Database.Migrate();

// ─── Run the Functions host ────────────────────────────────────────────
host.Run();




