using System.ComponentModel.DataAnnotations;

namespace QAWebApp.DTOs;

public class QuestionCreateDto
{
    [Required]
    [StringLength(200, MinimumLength = 10)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(5000, MinimumLength = 20)]
    public string Body { get; set; } = string.Empty;

    [Required]
    public string Tags { get; set; } = string.Empty; // Comma-separated tags
}

public class QuestionUpdateDto
{
    [Required]
    [StringLength(200, MinimumLength = 10)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(5000, MinimumLength = 20)]
    public string Body { get; set; } = string.Empty;

    [Required]
    public string Tags { get; set; } = string.Empty;
}
