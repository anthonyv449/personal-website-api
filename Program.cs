using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyIsolatedFuncApp;
using MyIsolatedFuncApp.Data;
using Microsoft.EntityFrameworkCore;


var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddDbContextFactory<MyDbContext>(options =>
        options.UseNpgsql(
            Environment.GetEnvironmentVariable("DefaultConnection")
        )
    );

builder.Build().Run();
