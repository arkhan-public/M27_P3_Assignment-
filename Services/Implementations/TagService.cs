using Microsoft.EntityFrameworkCore;
using QAWebApp.Data;
using QAWebApp.Models;
using QAWebApp.Services.Interfaces;

namespace QAWebApp.Services.Implementations;

public class TagService : ITagService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TagService> _logger;

    public TagService(ApplicationDbContext context, ILogger<TagService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Tag>> GetOrCreateTagsAsync(string tagsString)
    {
        try
        {
            var tagNames = tagsString.Split(',')
                .Select(t => t.Trim().ToLower())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .ToList();

            var tags = new List<Tag>();

            foreach (var tagName in tagNames)
            {
                var existingTag = await _context.Tags
                    .FirstOrDefaultAsync(t => t.Name == tagName);

                if (existingTag != null)
                {
                    tags.Add(existingTag);
                }
                else
                {
                    var newTag = new Tag
                    {
                        Name = tagName,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Tags.Add(newTag);
                    await _context.SaveChangesAsync();
                    tags.Add(newTag);
                    _logger.LogInformation("New tag created: {TagName}", tagName);
                }
            }

            return tags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting or creating tags");
            return new List<Tag>();
        }
    }

    public async Task<List<Tag>> GetAllTagsAsync()
    {
        try
        {
            return await _context.Tags
                .OrderBy(t => t.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all tags");
            return new List<Tag>();
        }
    }

    public async Task<List<Tag>> GetPopularTagsAsync(int count = 10)
    {
        try
        {
            return await _context.Tags
                .Include(t => t.Questions)
                .OrderByDescending(t => t.Questions.Count)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving popular tags");
            return new List<Tag>();
        }
    }
}
