using Microsoft.AspNetCore.Mvc.RazorPages;
using QAWebApp.Models;
using QAWebApp.Services.Interfaces;

namespace QAWebApp.Pages.Tags;

public class TagsIndexModel : PageModel
{
    private readonly ITagService _tagService;

    public TagsIndexModel(ITagService tagService)
    {
        _tagService = tagService;
    }

    public List<Tag> Tags { get; set; } = new();

    public async Task OnGetAsync()
    {
        Tags = await _tagService.GetPopularTagsAsync(50);
    }
}
