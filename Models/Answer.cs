using System.ComponentModel.DataAnnotations;

namespace QAWebApp.Models;

public class Answer
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Body { get; set; } = string.Empty;

    public int VoteCount { get; set; } = 0;

    public bool IsAccepted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Foreign keys
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    // Navigation properties
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
