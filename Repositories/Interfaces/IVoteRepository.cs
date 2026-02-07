using QAWebApp.Models;

namespace QAWebApp.Repositories.Interfaces;

public interface IVoteRepository : IRepository<Vote>
{
    Task<Vote?> GetVoteAsync(int userId, int? questionId, int? answerId);
    Task<List<Vote>> GetVotesByQuestionIdAsync(int questionId);
    Task<List<Vote>> GetVotesByAnswerIdAsync(int answerId);
    Task<int> GetQuestionVoteCountAsync(int questionId);
    Task<int> GetAnswerVoteCountAsync(int answerId);
}