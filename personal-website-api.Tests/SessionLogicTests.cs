using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using personal_website_api.Auth;
using UsersEntity = MyIsolatedFuncApp.Data.Users;
using MyDbContext = MyIsolatedFuncApp.Data.MyDbContext;

namespace personal_website_api.Tests
{
    public class SessionLogicTests
    {
        private static MyDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new MyDbContext(options);
        }

        [Fact]
        public async Task CreateAndRetrieveSession_Works()
        {
            using var context = CreateContext();
            var user = new UsersEntity { Name = "Jane", Email = "jane@example.com" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var sessionId = await CreateSessionLogic.Execute(context, user.Id);
            var result = await GetUserBySessionLogic.Execute(context, sessionId);

            Assert.NotNull(result);
            Assert.Equal(user.Email, result!.Email);
        }

        [Fact]
        public async Task GetUserBySession_ReturnsNull_WhenMissing()
        {
            using var context = CreateContext();
            var result = await GetUserBySessionLogic.Execute(context, "bad");
            Assert.Null(result);
        }
    }
}
