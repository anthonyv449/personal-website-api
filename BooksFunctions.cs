using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using personal_website_api.Books;

namespace personal_website_api
{
    public class BooksFunctions
    {
        private readonly ILogger<BooksFunctions> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _hardcoverToken;

        public BooksFunctions(
            ILogger<BooksFunctions> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            var apiKey = configuration["HARDCOVER_API_KEY"]
                      ?? configuration["Values:HARDCOVER_API_KEY"]
                      ?? string.Empty;

            logger.LogInformation("HARDCOVER_API_KEY raw length: {Len}, empty: {Empty}",
                apiKey.Length, string.IsNullOrWhiteSpace(apiKey));

            _hardcoverToken = apiKey.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? apiKey[7..]
                : apiKey;

            logger.LogInformation("Hardcover token length after strip: {Len}, first 10 chars: {Preview}",
                _hardcoverToken.Length,
                _hardcoverToken.Length > 10 ? _hardcoverToken[..10] + "…" : _hardcoverToken);
        }

        [Function("SearchBooks")]
        public async Task<HttpResponseData> SearchBooks(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "books")] HttpRequestData req)
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var title = query["title"];
            var author = query["author"];

            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(author))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("At least one filter (title or author) is required.");
                return bad;
            }

            _logger.LogInformation("Searching Hardcover books — title: {Title}, author: {Author}", title, author);

            var http = _httpClientFactory.CreateClient("hardcover");
            var books = await SearchBooksLogic.Execute(http, title, author, _hardcoverToken);

            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(books);
            return res;
        }
    }
}
