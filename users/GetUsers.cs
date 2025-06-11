using Microsoft.EntityFrameworkCore;
using MyIsolatedFuncApp.Data;
using UsersEntity = MyIsolatedFuncApp.Data.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace personal_website_api.Users
{
    public static class GetUsersLogic
    {
        public static async Task<List<UsersEntity>> Execute(MyDbContext db)
        {
            return await db.Users.ToListAsync();
        }
    }
}
