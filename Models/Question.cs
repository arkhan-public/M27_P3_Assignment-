using System.ComponentModel.DataAnnotations;

namespace QAWebApp.Models;

public class Question
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public int ViewCount { get; set; } = 0;

    public int VoteCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Foreign key
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    // Navigation properties
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
