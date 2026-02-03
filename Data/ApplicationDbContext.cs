using Microsoft.EntityFrameworkCore;
using QAWebApp.Models;
using BCrypt.Net;

namespace QAWebApp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationUser> Users { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Vote> Votes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configurations
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Question configurations
        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasOne(q => q.User)
                .WithMany(u => u.Questions)
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(q => q.Tags)
                .WithMany(t => t.Questions);

            entity.HasMany(q => q.Answers)
                .WithOne(a => a.Question)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(q => q.Comments)
                .WithOne(c => c.Question)
                .HasForeignKey(c => c.QuestionId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(q => q.Votes)
                .WithOne(v => v.Question)
                .HasForeignKey(v => v.QuestionId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Answer configurations
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasOne(a => a.User)
                .WithMany(u => u.Answers)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(a => a.Comments)
                .WithOne(c => c.Answer)
                .HasForeignKey(c => c.AnswerId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(a => a.Votes)
                .WithOne(v => v.Answer)
                .HasForeignKey(v => v.AnswerId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Comment configurations
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Vote configurations
        modelBuilder.Entity<Vote>(entity =>
        {
            entity.HasOne(v => v.User)
                .WithMany(u => u.Votes)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ensure unique vote per user per item
            entity.HasIndex(v => new { v.UserId, v.QuestionId, v.AnswerId }).IsUnique();
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed default tags
        var tags = new[]
        {
            new Tag { Id = 1, Name = "csharp", CreatedAt = DateTime.UtcNow },
            new Tag { Id = 2, Name = "aspnet", CreatedAt = DateTime.UtcNow },
            new Tag { Id = 3, Name = "sql", CreatedAt = DateTime.UtcNow },
            new Tag { Id = 4, Name = "ef-core", CreatedAt = DateTime.UtcNow },
            new Tag { Id = 5, Name = "jwt", CreatedAt = DateTime.UtcNow },
            new Tag { Id = 6, Name = "javascript", CreatedAt = DateTime.UtcNow },
            new Tag { Id = 7, Name = "html", CreatedAt = DateTime.UtcNow },
            new Tag { Id = 8, Name = "css", CreatedAt = DateTime.UtcNow }
        };
        modelBuilder.Entity<Tag>().HasData(tags);

        // Seed demo user
        var demoUser = new ApplicationUser
        {
            Id = 1,
            Username = "demo",
            Email = "demo@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo@123"),
            Reputation = 100,
            CreatedAt = DateTime.UtcNow
        };
        modelBuilder.Entity<ApplicationUser>().HasData(demoUser);

        // Seed demo question
        var demoQuestion = new Question
        {
            Id = 1,
            Title = "How to implement JWT authentication in ASP.NET Core?",
            Body = "I'm trying to implement JWT-based authentication in my ASP.NET Core application. What are the best practices and how do I configure it properly?",
            UserId = 1,
            ViewCount = 42,
            VoteCount = 5,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };
        modelBuilder.Entity<Question>().HasData(demoQuestion);

        // Seed demo answer
        var demoAnswer = new Answer
        {
            Id = 1,
            Body = "To implement JWT authentication in ASP.NET Core, you need to:\n\n1. Install Microsoft.AspNetCore.Authentication.JwtBearer NuGet package\n2. Configure JWT settings in appsettings.json\n3. Add authentication middleware in Program.cs\n4. Create a service to generate JWT tokens\n5. Protect your endpoints with [Authorize] attribute\n\nMake sure to use a strong secret key and configure token expiration appropriately.",
            QuestionId = 1,
            UserId = 1,
            VoteCount = 3,
            IsAccepted = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        modelBuilder.Entity<Answer>().HasData(demoAnswer);
    }
}
