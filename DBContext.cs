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
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Article> Articles { get; set; }
    }

    // Example entity
    public class Users
    {
        public int Id { get; set; }
        public string Name { get; set; } = "NA";
        public string Email { get; set; } = "NA";
    }

    public class Session
    {
        public string Id { get; set; } = string.Empty;
        public int UserId { get; set; }
    }

    public class Article
    {
        public int ArticleId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime DateUploaded { get; set; }
        public DateTime DateModified { get; set; }
        public bool Active { get; set; } = true;
        public string Owner { get; set; } = string.Empty;
        public string LastModifiedUser { get; set; } = string.Empty;
        public int Views { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string[] Tags { get; set; } = System.Array.Empty<string>();
        public string SeoTitle { get; set; } = string.Empty;
        public string SeoDescription { get; set; } = string.Empty;
        public string SeoKeywords { get; set; } = string.Empty;
        public string CanonicalUrl { get; set; } = string.Empty;
        public string SocialImageUrl { get; set; } = string.Empty;
    }
}
