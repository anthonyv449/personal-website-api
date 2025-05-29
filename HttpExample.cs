using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MyIsolatedFuncApp.Data;
using Microsoft.EntityFrameworkCore;  


namespace personal_website_api
{
    public class HttpExample
    {
        private readonly ILogger<HttpExample> _logger;
        private readonly MyDbContext _dbContext;

        public HttpExample(ILogger<HttpExample> logger, MyDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [Function("HttpExample")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]
            HttpRequestData req)
        {
            _logger.LogInformation("Processing HTTP request.");

            // Query the users
            var items = await _dbContext.Users.ToListAsync();

            // Create the response and serialize the items as JSON
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(items);

            return response;
        }
    }
}
