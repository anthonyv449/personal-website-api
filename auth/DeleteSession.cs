using System.Threading.Tasks;
using MyIsolatedFuncApp.Data;

namespace personal_website_api.Auth
{
    public static class DeleteSessionLogic
    {
        public static async Task Execute(MyDbContext db, string sessionId)
        {
            var session = await db.Sessions.FindAsync(sessionId);
            if (session != null)
            {
                db.Sessions.Remove(session);
                await db.SaveChangesAsync();
            }
        }
    }
}
