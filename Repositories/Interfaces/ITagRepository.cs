using QAWebApp.Models;

namespace QAWebApp.Repositories.Interfaces;

public interface ITagRepository : IRepository<Tag>
{
    Task<Tag?> GetByNameAsync(string name);
    Task<List<Tag>> GetTagsByNamesAsync(List<string> names);
    Task<List<Tag>> GetPopularTagsAsync(int count = 10);
}