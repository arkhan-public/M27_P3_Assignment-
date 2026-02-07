using Microsoft.EntityFrameworkCore;
using QAWebApp.Data;
using QAWebApp.Models;
using QAWebApp.Repositories.Interfaces;

namespace QAWebApp.Repositories.Implementations;

public class AnswerRepository : Repository<Answer>, IAnswerRepository
{
    public AnswerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Answer?> GetAnswerWithDetailsAsync(int answerId)
    {
        return await _dbSet
            .Include(a => a.User)
            .Include(a => a.Question)
            .Include(a => a.Comments)
                .ThenInclude(c => c.User)
            .Include(a => a.Votes)
            .FirstOrDefaultAsync(a => a.Id == answerId);
    }

    public async Task<List<Answer>> GetAnswersByQuestionIdAsync(int questionId)
    {
        return await _dbSet
            .Include(a => a.User)
            .Include(a => a.Comments)
                .ThenInclude(c => c.User)
            .Include(a => a.Votes)
            .Where(a => a.QuestionId == questionId)
            .OrderByDescending(a => a.IsAccepted)
            .ThenByDescending(a => a.VoteCount)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Answer>> GetAnswersByUserIdAsync(int userId)
    {
        return await _dbSet
            .Include(a => a.Question)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Answer?> GetAcceptedAnswerByQuestionIdAsync(int questionId)
    {
        return await _dbSet
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.QuestionId == questionId && a.IsAccepted);
    }
}