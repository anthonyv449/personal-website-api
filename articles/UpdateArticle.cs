using MyIsolatedFuncApp.Data;
using ArticleEntity = MyIsolatedFuncApp.Data.Article;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace personal_website_api.Articles
{
    public static class UpdateArticleLogic
    {
        public static async Task<ArticleEntity?> Execute(MyDbContext db, int id, ArticleEntity updated)
        {
            var article = await db.Articles.FindAsync(id);
            if (article == null)
            {
                return null;
            }

            article.Content = updated.Content;
            article.Author = updated.Author;
            article.DateUploaded = updated.DateUploaded;
            article.DateModified = updated.DateModified;
            article.Active = updated.Active;
            article.OwnerId = updated.OwnerId;
            article.LastModifiedUserId = updated.LastModifiedUserId;
            article.Views = updated.Views;
            article.Title = updated.Title;
            article.Slug = updated.Slug;
            article.Summary = updated.Summary;
            article.Tags = updated.Tags;
            article.SeoTitle = updated.SeoTitle;
            article.SeoDescription = updated.SeoDescription;
            article.SeoKeywords = updated.SeoKeywords;
            article.CanonicalUrl = updated.CanonicalUrl;
            article.SocialImageUrl = updated.SocialImageUrl;

            await db.SaveChangesAsync();
            return article;
        }
    }
}
