using System.ComponentModel.DataAnnotations;
using QAWebApp.Models;

namespace QAWebApp.DTOs;

public class VoteDto
{
    [Required]
    public VoteType Type { get; set; }

    public int? QuestionId { get; set; }

    public int? AnswerId { get; set; }
}
