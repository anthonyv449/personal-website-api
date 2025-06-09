using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using personal_website_api.Auth;
using UsersEntity = MyIsolatedFuncApp.Data.Users;
using MyDbContext = MyIsolatedFuncApp.Data.MyDbContext;

namespace personal_website_api.Tests
{
    public class LoginWithJwtLogicTests
    {
        private static MyDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new MyDbContext(options);
        }

        [Fact]
        public async Task Execute_CreatesUser_IfNotExists()
        {
            using var context = CreateContext();
            var user = await LoginWithJwtLogic.Execute(context, "Jane", "jane@example.com");
            Assert.Equal(1, await context.Users.CountAsync());
            Assert.Equal("jane@example.com", user.Email);
        }

        [Fact]
        public async Task Execute_ReturnsExistingUser()
        {
            using var context = CreateContext();
            context.Users.Add(new UsersEntity { Name = "Jane", Email = "jane@example.com" });
            await context.SaveChangesAsync();

            var user = await LoginWithJwtLogic.Execute(context, "Jane", "jane@example.com");
            Assert.Equal(1, await context.Users.CountAsync());
            Assert.Equal("jane@example.com", user.Email);
        }
    }
}
