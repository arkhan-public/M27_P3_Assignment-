using System.ComponentModel.DataAnnotations;

namespace QAWebApp.Models;

public enum VoteType
{
    Upvote = 1,
    Downvote = -1
}

public class Vote
{
    [Key]
    public int Id { get; set; }

    [Required]
    public VoteType Type { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public int? QuestionId { get; set; }
    public Question? Question { get; set; }

    public int? AnswerId { get; set; }
    public Answer? Answer { get; set; }
}
