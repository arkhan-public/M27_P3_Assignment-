using QAWebApp.DTOs;
using QAWebApp.Models;

namespace QAWebApp.Services.Interfaces;

public interface ICommentService
{
    Task<(bool Success, string Message, Comment? Comment)> CreateCommentAsync(CommentCreateDto dto, int userId);
    Task<List<Comment>> GetCommentsByQuestionIdAsync(int questionId);
    Task<List<Comment>> GetCommentsByAnswerIdAsync(int answerId);
    Task<(bool Success, string Message)> UpdateCommentAsync(int commentId, string body, int userId);
    Task<(bool Success, string Message)> DeleteCommentAsync(int commentId, int userId);
}
