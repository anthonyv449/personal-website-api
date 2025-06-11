using MyIsolatedFuncApp.Data;
using ArticleEntity = MyIsolatedFuncApp.Data.Article;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

namespace personal_website_api.Articles
{
    public static class UpdateArticleLogic
    {
        public static async Task<ArticleEntity?> Execute(MyDbContext db, int id, ArticleEntity updated, IDictionary<string, JsonElement>? fields = null)
        {
            var article = await db.Articles.FindAsync(id);
            if (article == null)
            {
                return null;
            }

            bool ShouldUpdate(string name) => fields == null || fields.Keys.Any(k => string.Equals(k, name, System.StringComparison.OrdinalIgnoreCase));

            if (ShouldUpdate(nameof(ArticleEntity.Content))) article.Content = updated.Content;
            if (ShouldUpdate(nameof(ArticleEntity.Author))) article.Author = updated.Author;
            if (ShouldUpdate(nameof(ArticleEntity.DateUploaded))) article.DateUploaded = updated.DateUploaded;
            if (ShouldUpdate(nameof(ArticleEntity.DateModified))) article.DateModified = updated.DateModified;
            if (ShouldUpdate(nameof(ArticleEntity.Active))) article.Active = updated.Active;
            if (ShouldUpdate(nameof(ArticleEntity.OwnerId))) article.OwnerId = updated.OwnerId;
            if (ShouldUpdate(nameof(ArticleEntity.LastModifiedUserId))) article.LastModifiedUserId = updated.LastModifiedUserId;
            if (ShouldUpdate(nameof(ArticleEntity.Views))) article.Views = updated.Views;
            if (ShouldUpdate(nameof(ArticleEntity.Title))) article.Title = updated.Title;
            if (ShouldUpdate(nameof(ArticleEntity.Slug))) article.Slug = updated.Slug;
            if (ShouldUpdate(nameof(ArticleEntity.Summary))) article.Summary = updated.Summary;
            if (ShouldUpdate(nameof(ArticleEntity.Tags))) article.Tags = updated.Tags;
            if (ShouldUpdate(nameof(ArticleEntity.SeoTitle))) article.SeoTitle = updated.SeoTitle;
            if (ShouldUpdate(nameof(ArticleEntity.SeoDescription))) article.SeoDescription = updated.SeoDescription;
            if (ShouldUpdate(nameof(ArticleEntity.SeoKeywords))) article.SeoKeywords = updated.SeoKeywords;
            if (ShouldUpdate(nameof(ArticleEntity.CanonicalUrl))) article.CanonicalUrl = updated.CanonicalUrl;
            if (ShouldUpdate(nameof(ArticleEntity.SocialImageUrl))) article.SocialImageUrl = updated.SocialImageUrl;

            await db.SaveChangesAsync();
            return article;
        }
    }
}
