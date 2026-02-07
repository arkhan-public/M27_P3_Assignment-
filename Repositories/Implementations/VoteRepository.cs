using Microsoft.EntityFrameworkCore;
using QAWebApp.Data;
using QAWebApp.Models;
using QAWebApp.Repositories.Interfaces;

namespace QAWebApp.Repositories.Implementations;

public class VoteRepository : Repository<Vote>, IVoteRepository
{
    public VoteRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Vote?> GetVoteAsync(int userId, int? questionId, int? answerId)
    {
        return await _dbSet.FirstOrDefaultAsync(v => 
            v.UserId == userId && 
            v.QuestionId == questionId && 
            v.AnswerId == answerId);
    }

    public async Task<List<Vote>> GetVotesByQuestionIdAsync(int questionId)
    {
        return await _dbSet
            .Where(v => v.QuestionId == questionId)
            .ToListAsync();
    }

    public async Task<List<Vote>> GetVotesByAnswerIdAsync(int answerId)
    {
        return await _dbSet
            .Where(v => v.AnswerId == answerId)
            .ToListAsync();
    }

    public async Task<int> GetQuestionVoteCountAsync(int questionId)
    {
        var votes = await _dbSet
            .Where(v => v.QuestionId == questionId)
            .ToListAsync();

        return votes.Sum(v => v.Type == VoteType.Upvote ? 1 : -1);
    }

    public async Task<int> GetAnswerVoteCountAsync(int answerId)
    {
        var votes = await _dbSet
            .Where(v => v.AnswerId == answerId)
            .ToListAsync();

        return votes.Sum(v => v.Type == VoteType.Upvote ? 1 : -1);
    }
}