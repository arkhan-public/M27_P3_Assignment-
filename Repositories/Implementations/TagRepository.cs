using Microsoft.EntityFrameworkCore;
using QAWebApp.Data;
using QAWebApp.Models;
using QAWebApp.Repositories.Interfaces;

namespace QAWebApp.Repositories.Implementations;

public class TagRepository : Repository<Tag>, ITagRepository
{
    public TagRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Tag?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
    }

    public async Task<List<Tag>> GetTagsByNamesAsync(List<string> names)
    {
        var lowerNames = names.Select(n => n.ToLower()).ToList();
        return await _dbSet
            .Where(t => lowerNames.Contains(t.Name.ToLower()))
            .ToListAsync();
    }

    public async Task<List<Tag>> GetPopularTagsAsync(int count = 10)
    {
        return await _dbSet
            .Include(t => t.Questions)
            .OrderByDescending(t => t.Questions.Count)
            .Take(count)
            .ToListAsync();
    }
}