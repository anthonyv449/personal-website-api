using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MyIsolatedFuncApp.Data;
using personal_website_api.Articles;
using personal_website_api.Auth;
using ArticleEntity = MyIsolatedFuncApp.Data.Article;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using Azure;
using Azure.AI.OpenAI;
using System.Collections.Generic;
using OpenAI.Chat;



namespace personal_website_api
{
    public class ArticleFunctions
    {
        private readonly ILogger<ArticleFunctions> _logger;
        private readonly MyDbContext _db;
        private readonly AzureOpenAIClient  _openAI;
        private readonly OpenAI.Chat.ChatClient _chatClient;
        private const string OpenAIModel = "gpt-35-turbo-article-creation";

        public ArticleFunctions(ILogger<ArticleFunctions> logger, MyDbContext db, AzureOpenAIClient openAI)
        {
            _logger = logger;
            _db = db;
            _openAI = openAI;
            _chatClient = openAI.GetChatClient(OpenAIModel);
        }


        [Function("GetArticles")]
        public async Task<HttpResponseData> GetArticles(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "articles/{slug?}")] HttpRequestData req,
            string? slug)
        {
            if (!string.IsNullOrEmpty(slug))
            {
                var article = await GetArticlesLogic.Execute(_db, slug);
                if (article == null)
                {
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }
                var singleRes = req.CreateResponse(HttpStatusCode.OK);
                await singleRes.WriteAsJsonAsync(article);
                return singleRes;
            }

            var articles = await GetArticlesLogic.Execute(_db);
            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(articles);
            return res;
        }

        [Function("CreateArticle")]
        public async Task<HttpResponseData> CreateArticle(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "articles")] HttpRequestData req)
        {
            var auth = await AuthorizationHelper.RequireAdmin(req, _db);
            if (auth != null) return auth;

            var user = await AuthorizationHelper.GetUserFromSession(req, _db);
            if (user == null)
            {
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }
            var bodyString = await new StreamReader(req.Body).ReadToEndAsync();
            var newArticle = JsonSerializer.Deserialize<Article>(bodyString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (newArticle == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
            newArticle.OwnerId = user.Id;
            newArticle.LastModifiedUserId = user.Id;

            var prompt = $"Fill in missing fields for an article with title: {newArticle.Title} and content: {newArticle.Content}. Return summary, SEO tags, SEO title, SEO description, SEO keywords.";
            ChatCompletion completion = _chatClient.CompleteChat(
                [
                    new SystemChatMessage("You are a helpful assistant that helps create and review articles."),
                    new UserChatMessage(prompt),
                ]);
            string aiResponse = completion.Content[0].Text;
            _logger.LogInformation("AI response content: {0}", aiResponse);
            newArticle.Summary = ExtractValue(aiResponse, "Summary:");
            var tagCsv = ExtractValue(aiResponse, "SEO Tags:");
            if (!string.IsNullOrWhiteSpace(tagCsv))
            {
                newArticle.Tags = tagCsv.Split(',', StringSplitOptions.RemoveEmptyEntries);
            }
            newArticle.SeoTitle = ExtractValue(aiResponse, "SEO Title:");
            newArticle.SeoDescription = ExtractValue(aiResponse, "SEO Description:");
            newArticle.SeoKeywords = ExtractValue(aiResponse, "SEO Keywords:");
            var readingTime = EstimateReadingTime(newArticle.Content);
            _logger.LogInformation("Estimated reading time: {Time}", readingTime);

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
            var auth = await AuthorizationHelper.RequireAdmin(req, _db);
            if (auth != null) return auth;

            string body = await new StreamReader(req.Body).ReadToEndAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var updated = JsonSerializer.Deserialize<ArticleEntity>(body, options);
            if (updated == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
            var fields = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(body, options);
            var result = await UpdateArticleLogic.Execute(_db, id, updated, fields);
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
            var auth = await AuthorizationHelper.RequireAdmin(req, _db);
            if (auth != null) return auth;

            var success = await DeleteArticleLogic.Execute(_db, id);
            return req.CreateResponse(success ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
        }

        private static string ExtractValue(string text, string label)
        {
            var start = text.IndexOf(label, StringComparison.OrdinalIgnoreCase);
            if (start == -1) return string.Empty;
            start += label.Length;
            var end = text.IndexOf('\n', start);
            if (end == -1) end = text.Length;
            return text.Substring(start, end - start).Trim();
        }

        private static string EstimateReadingTime(string? content)
        {
            if (string.IsNullOrWhiteSpace(content)) return "0 min read";
            var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            var minutes = Math.Max(1, (int)Math.Ceiling(words / 200.0));
            return $"{minutes} min read";
        }
    }
}
