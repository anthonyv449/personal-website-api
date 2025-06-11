using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using personal_website_api.Users;
using UsersEntity = MyIsolatedFuncApp.Data.Users;
using MyDbContext = MyIsolatedFuncApp.Data.MyDbContext;
using Xunit;

namespace personal_website_api.Tests
{
    public class UserLogicTests
    {
        private static MyDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new MyDbContext(options);
        }

        [Fact]
        public async Task CreateUser_AddsUser()
        {
            using var context = CreateContext();
            var user = new UsersEntity { Name = "Jane", Email = "jane@example.com" };
            await CreateUserLogic.Execute(context, user);

            Assert.Equal(1, await context.Users.CountAsync());
        }

        [Fact]
        public async Task GetUsers_ReturnsAll()
        {
            using var context = CreateContext();
            context.Users.Add(new UsersEntity { Name = "A", Email = "a@example.com" });
            context.Users.Add(new UsersEntity { Name = "B", Email = "b@example.com" });
            await context.SaveChangesAsync();

            var users = await GetUsersLogic.Execute(context);
            Assert.Equal(2, users.Count);
        }

        [Fact]
        public async Task UpdateUser_UpdatesFields()
        {
            using var context = CreateContext();
            var user = new UsersEntity { Name = "Old", Email = "old@example.com" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var updated = new UsersEntity { Name = "New", Email = "new@example.com" };
            var result = await UpdateUserLogic.Execute(context, user.Id, updated);

            Assert.NotNull(result);
            Assert.Equal("New", result!.Name);
            Assert.Equal("new@example.com", result.Email);
        }

        [Fact]
        public async Task DeleteUser_Removes()
        {
            using var context = CreateContext();
            var user = new UsersEntity { Name = "X", Email = "x@example.com" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var success = await DeleteUserLogic.Execute(context, user.Id);

            Assert.True(success);
            Assert.Empty(await context.Users.ToListAsync());
        }

        [Fact]
        public async Task DeleteUser_ReturnsFalse_WhenMissing()
        {
            using var context = CreateContext();
            var success = await DeleteUserLogic.Execute(context, 1);
            Assert.False(success);
        }
    }
}
