using MyIsolatedFuncApp.Data;
using ArticleEntity = MyIsolatedFuncApp.Data.Article;
using System.Threading.Tasks;

namespace personal_website_api.Articles
{
    public static class DeleteArticleLogic
    {
        public static async Task<bool> Execute(MyDbContext db, int id)
        {
            var article = await db.Articles.FindAsync(id);
            if (article == null)
            {
                return false;
            }

            db.Articles.Remove(article);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
