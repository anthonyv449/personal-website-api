using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyIsolatedFuncApp.Data;
using Microsoft.EntityFrameworkCore;


namespace personal_website_api;

public class HttpExample
{
    private readonly ILogger<HttpExample> _logger;
    private readonly MyDbContext _dbContext;


    public HttpExample( MyDbContext dbContext,   ILogger<HttpExample> logger)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [Function("HttpExample")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("Fetching items from Postgres");

        var items = await _dbContext.Users.ToListAsync();

        return new OkObjectResult(items);
    }
}
