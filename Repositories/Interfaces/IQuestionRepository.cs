using QAWebApp.Models;

namespace QAWebApp.Repositories.Interfaces;

public interface IQuestionRepository : IRepository<Question>
{
    Task<Question?> GetQuestionWithDetailsAsync(int questionId);
    Task<List<Question>> GetAllQuestionsWithDetailsAsync(string? searchTerm = null, string? tag = null);
    Task<List<Question>> GetLatestQuestionsAsync(int count = 10);
    Task<List<Question>> GetQuestionsByUserIdAsync(int userId);
    Task IncrementViewCountAsync(int questionId);
}