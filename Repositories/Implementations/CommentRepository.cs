using Microsoft.EntityFrameworkCore;
using QAWebApp.Data;
using QAWebApp.Models;
using QAWebApp.Repositories.Interfaces;

namespace QAWebApp.Repositories.Implementations;

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<Comment>> GetCommentsByQuestionIdAsync(int questionId)
    {
        return await _dbSet
            .Include(c => c.User)
            .Where(c => c.QuestionId == questionId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Comment>> GetCommentsByAnswerIdAsync(int answerId)
    {
        return await _dbSet
            .Include(c => c.User)
            .Where(c => c.AnswerId == answerId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Comment?> GetCommentWithDetailsAsync(int commentId)
    {
        return await _dbSet
            .Include(c => c.User)
            .Include(c => c.Question)
            .Include(c => c.Answer)
            .FirstOrDefaultAsync(c => c.Id == commentId);
    }
}