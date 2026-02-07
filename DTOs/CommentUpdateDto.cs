using System.ComponentModel.DataAnnotations;

namespace QAWebApp.DTOs;

public class CommentUpdateDto
{
    [Required]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Comment must be between 5 and 500 characters")]
    public string Body { get; set; } = string.Empty;
}