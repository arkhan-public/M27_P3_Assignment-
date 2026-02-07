using QAWebApp.Models;

namespace QAWebApp.Repositories.Interfaces;

public interface ICommentRepository : IRepository<Comment>
{
    Task<List<Comment>> GetCommentsByQuestionIdAsync(int questionId);
    Task<List<Comment>> GetCommentsByAnswerIdAsync(int answerId);
    Task<Comment?> GetCommentWithDetailsAsync(int commentId);
}