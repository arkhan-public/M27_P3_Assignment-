using Microsoft.AspNetCore.Mvc.RazorPages;
using QAWebApp.Models;
using QAWebApp.Services.Interfaces;

namespace QAWebApp.Pages;

public class IndexModel : PageModel
{
    private readonly IQuestionService _questionService;
    private readonly ITagService _tagService;

    public IndexModel(IQuestionService questionService, ITagService tagService)
    {
        _questionService = questionService;
        _tagService = tagService;
    }

    public List<Question> Questions { get; set; } = new();
    public List<Tag> PopularTags { get; set; } = new();

    public async Task OnGetAsync()
    {
        Questions = await _questionService.GetLatestQuestionsAsync(10);
        PopularTags = await _tagService.GetPopularTagsAsync(10);
    }
}
