using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using personal_website_api.Articles;
using ArticleEntity = MyIsolatedFuncApp.Data.Article;
using MyDbContext = MyIsolatedFuncApp.Data.MyDbContext;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace personal_website_api.Tests
{
    public class ArticleLogicTests
    {
        private static MyDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new MyDbContext(options);
        }

        [Fact]
        public async Task CreateArticle_AddsArticle()
        {
            using var context = CreateContext();
            var article = new ArticleEntity { Title = "Hello", Content = "Body" };
            var result = await CreateArticleLogic.Execute(context, article);

            Assert.Equal(1, await context.Articles.CountAsync());
            Assert.True((DateTime.UtcNow - result.DateUploaded).TotalSeconds < 5);
            Assert.True((DateTime.UtcNow - result.DateModified).TotalSeconds < 5);
        }

        [Fact]
        public async Task GetArticles_ReturnsAll()
        {
            using var context = CreateContext();
            context.Articles.Add(new ArticleEntity { Title = "A" });
            context.Articles.Add(new ArticleEntity { Title = "B" });
            await context.SaveChangesAsync();

            var articles = await GetArticlesLogic.Execute(context);
            Assert.Equal(2, articles.Count);
        }

        [Fact]
        public async Task UpdateArticle_UpdatesFields()
        {
            using var context = CreateContext();
            var article = new ArticleEntity { Title = "Old", Content = "X" };
            context.Articles.Add(article);
            await context.SaveChangesAsync();

            var updated = new ArticleEntity { Title = "New", Content = "Y" };
            var fields = new System.Collections.Generic.Dictionary<string, System.Text.Json.JsonElement>
            {
                ["title"] = default,
                ["content"] = default
            };
            var result = await UpdateArticleLogic.Execute(context, article.ArticleId, updated, fields);

            Assert.NotNull(result);
            Assert.Equal("New", result!.Title);
            Assert.Equal("Y", result.Content);
        }

        [Fact]
        public async Task UpdateArticle_DoesNotChangeMissingFields()
        {
            using var context = CreateContext();
            var article = new ArticleEntity { Title = "Old", Content = "X", Author = "Bob" };
            context.Articles.Add(article);
            await context.SaveChangesAsync();

            var updated = new ArticleEntity { Title = "New" };
            var fields = new System.Collections.Generic.Dictionary<string, System.Text.Json.JsonElement>
            {
                ["title"] = default
            };
            var result = await UpdateArticleLogic.Execute(context, article.ArticleId, updated, fields);

            Assert.NotNull(result);
            Assert.Equal("New", result!.Title);
            Assert.Equal("X", result.Content); // unchanged
            Assert.Equal("Bob", result.Author); // unchanged
        }

        [Fact]
        public async Task DeleteArticle_Removes()
        {
            using var context = CreateContext();
            var article = new ArticleEntity { Title = "X" };
            context.Articles.Add(article);
            await context.SaveChangesAsync();

            var success = await DeleteArticleLogic.Execute(context, article.ArticleId);

            Assert.True(success);
            Assert.Empty(await context.Articles.ToListAsync());
        }

        [Fact]
        public async Task DeleteArticle_ReturnsFalse_WhenMissing()
        {
            using var context = CreateContext();
            var success = await DeleteArticleLogic.Execute(context, 1);
            Assert.False(success);
        }
    }
}
