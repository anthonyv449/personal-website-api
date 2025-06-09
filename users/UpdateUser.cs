using MyIsolatedFuncApp.Data;
using UsersEntity = MyIsolatedFuncApp.Data.Users;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace personal_website_api.Users
{
    public static class UpdateUserLogic
    {
        public static async Task<UsersEntity?> Execute(MyDbContext db, int id, UsersEntity updated)
        {
            var user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return null;
            }

            user.Name = updated.Name;
            user.Email = updated.Email;
            await db.SaveChangesAsync();
            return user;
        }
    }
}
