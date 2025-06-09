using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using personal_website_api.Auth;
using UsersEntity = MyIsolatedFuncApp.Data.Users;
using MyDbContext = MyIsolatedFuncApp.Data.MyDbContext;

namespace personal_website_api.Tests
{
    public class DeleteSessionLogicTests
    {
        private static MyDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new MyDbContext(options);
        }

        [Fact]
        public async Task Execute_RemovesSession()
        {
            using var context = CreateContext();
            var user = new UsersEntity { Name = "Jane", Email = "jane@example.com" };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            var sessionId = await CreateSessionLogic.Execute(context, user.Id);

            await DeleteSessionLogic.Execute(context, sessionId);

            var session = await context.Sessions.FindAsync(sessionId);
            Assert.Null(session);
        }
    }
}
