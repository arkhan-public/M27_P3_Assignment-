using QAWebApp.Models;

namespace QAWebApp.Services.Interfaces;

public interface ITagService
{
    Task<List<Tag>> GetOrCreateTagsAsync(string tagsString);
    Task<List<Tag>> GetAllTagsAsync();
    Task<List<Tag>> GetPopularTagsAsync(int count = 10);
}
