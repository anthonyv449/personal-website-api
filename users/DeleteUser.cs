using MyIsolatedFuncApp.Data;
using UsersEntity = MyIsolatedFuncApp.Data.Users;
using System.Threading.Tasks;

namespace personal_website_api.Users
{
    public static class DeleteUserLogic
    {
        public static async Task<bool> Execute(MyDbContext db, int id)
        {
            var user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            db.Users.Remove(user);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
