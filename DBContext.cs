using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = "NA";
        [Required]
        public string Email { get; set; } = "NA";
    }

    public class Session
    {
        [Key]
        [Required]
        public string Id { get; set; } = string.Empty;
        [Required]
        public int UserId { get; set; }
    }

      public class Article
    {
        [Key]
        [Required]
        public int ArticleId { get; set; } // required and database generated
        public string? Content { get; set; } // nullable
        public string? Author { get; set; } // nullable
        public DateTime DateUploaded { get; set; } // populated on create
        public DateTime DateModified { get; set; } // populated on update
        public bool Active { get; set; } = true; // non-nullable
        public int OwnerId { get; set; } // maps to Users table
        public int LastModifiedUserId { get; set; } // maps to Users table
        public int? Views { get; set; } // nullable

        public string Title { get; set; } = string.Empty; // non-nullable
        public string Slug { get; set; } = string.Empty; // required
        public string? Summary { get; set; } // nullable
        public string[]? Tags { get; set; } // nullable
        public string? SeoTitle { get; set; } // nullable
        public string? SeoDescription { get; set; } // nullable
        public string? SeoKeywords { get; set; } // nullable
        public string? CanonicalUrl { get; set; } // nullable
        public string? SocialImageUrl { get; set; } // nullable
    }
}
