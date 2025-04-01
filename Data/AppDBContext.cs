using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Optional: seed data
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Name = "Test User", Email = "test@example.com" }
        );
    }
}
