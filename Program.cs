using System;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker.Configuration;
using Microsoft.EntityFrameworkCore;
using MyIsolatedFuncApp;      // your root namespace
using MyIsolatedFuncApp.Data; // where MyDbContext lives
using personal_website_api;

Console.WriteLine("🚀 Function App Host starting...");

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
            Console.WriteLine("🧪 Starting ConfigureServices...");

            var conn = context.Configuration.GetConnectionString("PostgresConnection");
            Console.WriteLine($"🔗 Resolved connection string: {(string.IsNullOrEmpty(conn) ? "[EMPTY]" : "[OK]")}");

            if (string.IsNullOrWhiteSpace(conn))
                throw new InvalidOperationException("❌ Postgres connection string is not configured!");

            // services.AddDbContextFactory<MyDbContext>(opts =>
            // {
            //     Console.WriteLine("🧱 Configuring DbContextFactory...");
            //     opts.UseNpgsql(conn);
            // });

            services.AddTransient<HttpExample>();

            Console.WriteLine("✅ ConfigureServices finished successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Fatal startup exception: {ex}");
            throw;
        }
    })
    .Build();

// ── auto-migrate pending EF Core migrations on cold start ──
// using (var scope = host.Services.CreateScope())
// {
//     var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MyDbContext>>();
//     await using var db = factory.CreateDbContext();
//     db.Database.Migrate();
// }

host.Run();
