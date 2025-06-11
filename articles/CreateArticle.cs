using MyIsolatedFuncApp.Data;
using ArticleEntity = MyIsolatedFuncApp.Data.Article;
using System.Threading.Tasks;
using System;

namespace personal_website_api.Articles
{
    public static class CreateArticleLogic
    {
        public static async Task<ArticleEntity> Execute(MyDbContext db, ArticleEntity article)
        {
            var now = DateTime.UtcNow;
            article.DateUploaded = now;
            article.DateModified = now;

            db.Articles.Add(article);
            await db.SaveChangesAsync();
            return article;
        }
    }
}
