using QAWebApp.Models;

namespace QAWebApp.Repositories.Interfaces;

public interface IAnswerRepository : IRepository<Answer>
{
    Task<Answer?> GetAnswerWithDetailsAsync(int answerId);
    Task<List<Answer>> GetAnswersByQuestionIdAsync(int questionId);
    Task<List<Answer>> GetAnswersByUserIdAsync(int userId);
    Task<Answer?> GetAcceptedAnswerByQuestionIdAsync(int questionId);
}