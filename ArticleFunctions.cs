using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MyIsolatedFuncApp.Data;
using personal_website_api.Articles;
using ArticleEntity = MyIsolatedFuncApp.Data.Article;

namespace personal_website_api
{
    public class ArticleFunctions
    {
        private readonly ILogger<ArticleFunctions> _logger;
        private readonly MyDbContext _db;

        public ArticleFunctions(ILogger<ArticleFunctions> logger, MyDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        [Function("GetArticles")]
        public async Task<HttpResponseData> GetArticles(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "articles")] HttpRequestData req)
        {
            var articles = await GetArticlesLogic.Execute(_db);
            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(articles);
            return res;
        }

        [Function("CreateArticle")]
        public async Task<HttpResponseData> CreateArticle(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "articles")] HttpRequestData req)
        {
            var newArticle = await req.ReadFromJsonAsync<ArticleEntity>();
            if (newArticle == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
            var created = await CreateArticleLogic.Execute(_db, newArticle);
            var res = req.CreateResponse(HttpStatusCode.Created);
            await res.WriteAsJsonAsync(created);
            return res;
        }

        [Function("UpdateArticle")]
        public async Task<HttpResponseData> UpdateArticle(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "articles/{id:int}")] HttpRequestData req,
            int id)
        {
            var updated = await req.ReadFromJsonAsync<ArticleEntity>();
            if (updated == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
            var result = await UpdateArticleLogic.Execute(_db, id, updated);
            if (result == null)
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }
            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(result);
            return res;
        }

        [Function("DeleteArticle")]
        public async Task<HttpResponseData> DeleteArticle(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "articles/{id:int}")] HttpRequestData req,
            int id)
        {
            var success = await DeleteArticleLogic.Execute(_db, id);
            return req.CreateResponse(success ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
        }
    }
}
