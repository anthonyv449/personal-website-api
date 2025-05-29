using Microsoft.EntityFrameworkCore;

namespace MyIsolatedFuncApp.Data
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {
        }

        // Define your tables here:
        public DbSet<Users> Users { get; set; }
    }

    // Example entity
    public class Users
    {
        public int Id { get; set; }
        public string Name { get; set; } = "NA";
        public string Email { get; set; } = "NA";
    }
}
