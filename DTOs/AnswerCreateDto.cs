using System.ComponentModel.DataAnnotations;

namespace QAWebApp.DTOs;

public class AnswerCreateDto
{
    [Required]
    [StringLength(5000, MinimumLength = 20)]
    public string Body { get; set; } = string.Empty;

    [Required]
    public int QuestionId { get; set; }
}

public class AnswerUpdateDto
{
    [Required]
    [StringLength(5000, MinimumLength = 20)]
    public string Body { get; set; } = string.Empty;
}
