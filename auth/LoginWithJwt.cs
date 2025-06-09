using MyIsolatedFuncApp.Data;
using Microsoft.EntityFrameworkCore;
using UsersEntity = MyIsolatedFuncApp.Data.Users;
using System.Threading.Tasks;

namespace personal_website_api.Auth
{
    public static class LoginWithJwtLogic
    {
        public static async Task<UsersEntity> Execute(MyDbContext db, string name, string email)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = new UsersEntity { Name = name, Email = email };
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }
            return user;
        }
    }
}
