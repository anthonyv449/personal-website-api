using System.Threading.Tasks;
using MyIsolatedFuncApp.Data;

namespace personal_website_api.Auth
{
    public static class CreateSessionLogic
    {
        public static async Task<string> Execute(MyDbContext db, int userId)
        {
            var session = new Session { Id = System.Guid.NewGuid().ToString(), UserId = userId };
            db.Sessions.Add(session);
            await db.SaveChangesAsync();
            return session.Id;
        }
    }
}
