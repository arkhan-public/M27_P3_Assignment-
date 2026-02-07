using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QAWebApp.Models;
using QAWebApp.Services.Interfaces;

namespace QAWebApp.Pages.Answers;

public class EditModel : PageModel
{
    private readonly IAnswerService _answerService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(IAnswerService answerService, ILogger<EditModel> logger)
    {
        _answerService = answerService;
        _logger = logger;
    }

    public Answer? Answer { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Answer = await _answerService.GetAnswerByIdAsync(id);
        
        if (Answer == null)
        {
            return NotFound();
        }

        // Note: we do not validate ownership server-side here because JWT is stored in client localStorage
        // and not automatically sent with this page request. Ownership is enforced by API on PUT/DELETE.
        return Page();
    }
}