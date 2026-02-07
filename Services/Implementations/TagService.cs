using QAWebApp.Models;
using QAWebApp.Repositories.Interfaces;
using QAWebApp.Services.Interfaces;

namespace QAWebApp.Services.Implementations;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<TagService> _logger;

    public TagService(ITagRepository tagRepository, ILogger<TagService> logger)
    {
        _tagRepository = tagRepository;
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
                var existingTag = await _tagRepository.GetByNameAsync(tagName);

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
                    await _tagRepository.AddAsync(newTag);
                    await _tagRepository.SaveChangesAsync();
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
            var tags = await _tagRepository.GetAllAsync();
            return tags.OrderBy(t => t.Name).ToList();
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
            return await _tagRepository.GetPopularTagsAsync(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving popular tags");
            return new List<Tag>();
        }
    }
}
