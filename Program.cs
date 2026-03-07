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
using personal_website_api.Auth;
using personal_website_api.Articles;
using personal_website_api.Publishers;
using personal_website_api.Subscribers;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Azure.AI.OpenAI;
using Azure;


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

            var conn = context.Configuration["PostgresConnection"] ??
                       context.Configuration["Values:PostgresConnection"];
            Console.WriteLine($"🔗 Resolved connection string: {(string.IsNullOrWhiteSpace(conn) ? "[EMPTY]" : "[OK]")}");

            if (string.IsNullOrWhiteSpace(conn))
                throw new InvalidOperationException("❌ Postgres connection string is not configured!");

            services.AddDbContextFactory<MyDbContext>(opts =>
            {
                Console.WriteLine("🧱 Configuring DbContextFactory...");
                opts.UseNpgsql(conn);
            });

            var sbConn = context.Configuration["ServiceBusConnection"] ??
                         context.Configuration["Values:ServiceBusConnection"];
            services.AddSingleton(new ServiceBusClient(sbConn));
            services.AddSingleton(new ServiceBusAdministrationClient(sbConn));
            services.AddSingleton<IServiceBusMessageSender, ServiceBusMessageSender>();

            var oaiEndpoint = context.Configuration["AZURE_OPENAI_ENDPOINT"] ??
                             context.Configuration["Values:AZURE_OPENAI_ENDPOINT"];
            var oaiKey = context.Configuration["AZURE_OPENAI_KEY"] ??
                          context.Configuration["Values:AZURE_OPENAI_KEY"];
            services.AddSingleton(new AzureOpenAIClient(new Uri(oaiEndpoint), new AzureKeyCredential(oaiKey)));

            services.AddHttpClient("hardcover");

            services.AddTransient<ArticleViewPublisher>();
            services.AddTransient<ArticleViewSubscriber>();

            services.AddTransient<HttpExample>(); // or your function class using DbContext
            services.AddTransient<UsersFunctions>();
            services.AddTransient<ArticleFunctions>();
            services.AddTransient<BooksFunctions>();
            services.AddTransient<AuthFunctions>();
            services.AddTransient<ITokenValidator, AzureAdTokenValidator>();
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
try
{
    using var scope = host.Services.CreateScope();
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MyDbContext>>();
    await using var db = factory.CreateDbContext();
    Console.WriteLine("🔁 Applying EF Core migrations...");
    db.Database.Migrate();
    Console.WriteLine("✅ Migrations applied.");

    var admin = scope.ServiceProvider.GetRequiredService<ServiceBusAdministrationClient>();
    await EnsureServiceBusEntities(admin);
}
catch (Exception ex)
{
    Console.WriteLine($"💥 Migration failure: {ex.Message}");
    // optionally rethrow if you want to fail the app
}


host.Run();

static async Task EnsureServiceBusEntities(ServiceBusAdministrationClient admin)
{
    const string topic = "article-views";
    const string subscription = "view-updater";

    if (!await admin.TopicExistsAsync(topic))
    {
        await admin.CreateTopicAsync(topic);
    }

    if (!await admin.SubscriptionExistsAsync(topic, subscription))
    {
        await admin.CreateSubscriptionAsync(topic, subscription);
    }
}
