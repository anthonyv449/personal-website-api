using MyIsolatedFuncApp.Data;
using UsersEntity = MyIsolatedFuncApp.Data.Users;
using System.Threading.Tasks;

namespace personal_website_api.Users
{
    public static class CreateUserLogic
    {
        public static async Task<UsersEntity> Execute(MyDbContext db, UsersEntity user)
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return user;
        }
    }
}
