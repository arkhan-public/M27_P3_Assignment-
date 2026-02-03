using System.ComponentModel.DataAnnotations;

namespace QAWebApp.Models;

public class Comment
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(500)]
    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public int? QuestionId { get; set; }
    public Question? Question { get; set; }

    public int? AnswerId { get; set; }
    public Answer? Answer { get; set; }
}
