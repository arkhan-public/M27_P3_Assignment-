using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QAWebApp.Models;
using QAWebApp.Services.Interfaces;

namespace QAWebApp.Pages.Answers;

public class EditModel : PageModel
{
    private readonly IAnswerService _answerService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(IAnswerService answerService, IJwtService jwtService, ILogger<EditModel> logger)
    {
        _answerService = answerService;
        _jwtService = jwtService;
        _logger = logger;
    }

    public Answer? Answer { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        // Read JWT from cookie (client must set cookie on login). Server-side ownership will be enforced here.
        var token = Request.Cookies["jwtToken"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToPage("/Login");
        }

        var userId = _jwtService.ValidateToken(token);
        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        Answer = await _answerService.GetAnswerByIdAsync(id);
        if (Answer == null)
        {
            return NotFound();
        }

        if (Answer.UserId != userId.Value)
        {
            _logger.LogWarning("User {UserId} attempted to access edit page for answer {AnswerId} owned by {OwnerId}", userId.Value, id, Answer.UserId);
            return Forbid();
        }

        return Page();
    }
}