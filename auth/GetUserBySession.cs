using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyIsolatedFuncApp.Data;
using UsersEntity = MyIsolatedFuncApp.Data.Users;

namespace personal_website_api.Auth
{
    public static class GetUserBySessionLogic
    {
        public static async Task<UsersEntity?> Execute(MyDbContext db, string sessionId)
        {
            var session = await db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null) return null;
            return await db.Users.FirstOrDefaultAsync(u => u.Id == session.UserId);
        }
    }
}
