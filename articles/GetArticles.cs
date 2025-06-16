using Microsoft.EntityFrameworkCore;
using MyIsolatedFuncApp.Data;
using ArticleEntity = MyIsolatedFuncApp.Data.Article;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace personal_website_api.Articles
{
    public static class GetArticlesLogic
    {
        public static async Task<List<ArticleEntity>> Execute(MyDbContext db)
        {
            return await db.Articles.ToListAsync();
        }

        public static async Task<ArticleEntity?> Execute(MyDbContext db, string slug)
        {
            return await db.Articles.FirstOrDefaultAsync(a => a.Slug == slug);
        }
    }
}
