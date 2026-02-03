using System.ComponentModel.DataAnnotations;

namespace QAWebApp.Models;

public class Tag
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
