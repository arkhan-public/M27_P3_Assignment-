using Microsoft.EntityFrameworkCore;
using QAWebApp.Data;
using QAWebApp.Models;
using QAWebApp.Repositories.Interfaces;

namespace QAWebApp.Repositories.Implementations;

public class QuestionRepository : Repository<Question>, IQuestionRepository
{
    public QuestionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Question?> GetQuestionWithDetailsAsync(int questionId)
    {
        return await _dbSet
            .Include(q => q.User)
            .Include(q => q.Tags)
            .Include(q => q.Answers)
                .ThenInclude(a => a.User)
            .Include(q => q.Answers)
                .ThenInclude(a => a.Comments)
                    .ThenInclude(c => c.User)
            .Include(q => q.Comments)
                .ThenInclude(c => c.User)
            .Include(q => q.Votes)
            .FirstOrDefaultAsync(q => q.Id == questionId);
    }

    public async Task<List<Question>> GetAllQuestionsWithDetailsAsync(string? searchTerm = null, string? tag = null)
    {
        var query = _dbSet
            .Include(q => q.User)
            .Include(q => q.Tags)
            .Include(q => q.Answers)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(q => q.Title.Contains(searchTerm) || q.Body.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            query = query.Where(q => q.Tags.Any(t => t.Name == tag));
        }

        return await query
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Question>> GetLatestQuestionsAsync(int count = 10)
    {
        return await _dbSet
            .Include(q => q.User)
            .Include(q => q.Tags)
            .Include(q => q.Answers)
            .OrderByDescending(q => q.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Question>> GetQuestionsByUserIdAsync(int userId)
    {
        return await _dbSet
            .Include(q => q.Tags)
            .Include(q => q.Answers)
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task IncrementViewCountAsync(int questionId)
    {
        var question = await _dbSet.FindAsync(questionId);
        if (question != null)
        {
            question.ViewCount++;
        }
    }
}