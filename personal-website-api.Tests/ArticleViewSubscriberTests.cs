using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using personal_website_api.Subscribers;
using MyIsolatedFuncApp.Data;

namespace personal_website_api.Tests;

public class ArticleViewSubscriberTests
{
    private class TestFactory : IDbContextFactory<MyDbContext>
    {
        private readonly DbContextOptions<MyDbContext> _options;
        public TestFactory(DbContextOptions<MyDbContext> options) => _options = options;
        public MyDbContext CreateDbContext() => new MyDbContext(_options);
    }

    [Fact]
    public async Task ProcessMessageAsync_IncrementsViews()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var factory = new TestFactory(options);

        using (var db = factory.CreateDbContext())
        {
            db.Articles.Add(new Article { ArticleId = 1, Views = 0 });
            await db.SaveChangesAsync();
        }

        var subscriber = new ArticleViewSubscriber(factory, NullLogger<ArticleViewSubscriber>.Instance);
        var msg = new ArticleViewMessage(1, "u", DateTime.UtcNow);

        await subscriber.ProcessMessageAsync(msg);

        using var check = factory.CreateDbContext();
        var updated = await check.Articles.FindAsync(1);
        Assert.Equal(1, updated!.Views);
        Assert.NotNull(updated.LastViewedAt);
    }
}
