using System.ComponentModel.DataAnnotations;

namespace QAWebApp.DTOs;

public class CommentCreateDto
{
    [Required]
    [StringLength(500, MinimumLength = 5)]
    public string Body { get; set; } = string.Empty;

    public int? QuestionId { get; set; }

    public int? AnswerId { get; set; }
}
