using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyIsolatedFuncApp.Data;

namespace personal_website_api.Subscribers;

public class ArticleViewSubscriber
{
    private readonly IDbContextFactory<MyDbContext> _dbContextFactory;
    private readonly ILogger<ArticleViewSubscriber> _logger;

    public ArticleViewSubscriber(IDbContextFactory<MyDbContext> dbContextFactory, ILogger<ArticleViewSubscriber> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    [Function("UpdateArticleView")]
    public async Task Run(
        [ServiceBusTrigger("article-views", "view-updater", Connection = "ServiceBusConnection")] string messageBody)
    {
        var data = JsonSerializer.Deserialize<ArticleViewMessage>(messageBody);
        await ProcessMessageAsync(data);
    }

    public async Task ProcessMessageAsync(ArticleViewMessage data)
    {
        await using var db = _dbContextFactory.CreateDbContext();
        var article = await db.Articles.FindAsync(data.ArticleId);
        if (article != null)
        {
            article.Views = (article.Views ?? 0) + 1;
            article.LastViewedAt = data.ViewedAt;
            await db.SaveChangesAsync();
        }

        _logger.LogInformation("Article {ArticleId} view recorded.", data.ArticleId);
    }
}
