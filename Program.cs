using System;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker.Configuration;
using Microsoft.EntityFrameworkCore;
using MyIsolatedFuncApp;      // your root namespace
using MyIsolatedFuncApp.Data; // where MyDbContext lives

var host = Host.CreateDefaultBuilder(args)
    // 1) load local.settings.json (for local) and env vars (for Azure)
    .ConfigureAppConfiguration((ctx, config) =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })

    // 2) hook up attribute-based Functions (HTTP, Timer, EventGrid, etc.)
    .ConfigureFunctionsWorkerDefaults()

    // 3) register your DbContext factory
    .ConfigureServices((context, services) =>
    {
        try
        {
            var conn = context.Configuration.GetConnectionString("PostgresConnection");
            Console.WriteLine($"[DEBUG] Connection string length: {conn?.Length ?? 0}");

            if (string.IsNullOrWhiteSpace(conn))
                throw new InvalidOperationException("PostgresConnection is not configured!");

            services.AddDbContextFactory<MyDbContext>(opts =>
                opts.UseNpgsql(conn)
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FATAL] Startup exception: {ex.Message}");
            throw;
        }
    })
    .Build();

// ── auto-migrate pending EF Core migrations on cold start ──
using (var scope = host.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MyDbContext>>();
    await using var db = factory.CreateDbContext();
    db.Database.Migrate();
}

host.Run();
