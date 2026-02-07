using QAWebApp.DTOs;
using QAWebApp.Models;
using QAWebApp.Repositories.Interfaces;
using QAWebApp.Services.Interfaces;

namespace QAWebApp.Services.Implementations;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ILogger<CommentService> _logger;

    public CommentService(ICommentRepository commentRepository, ILogger<CommentService> logger)
    {
        _commentRepository = commentRepository;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, Comment? Comment)> CreateCommentAsync(CommentCreateDto dto, int userId)
    {
        try
        {
            if (dto.QuestionId == null && dto.AnswerId == null)
            {
                return (false, "Comment must be associated with a question or answer", null);
            }

            var comment = new Comment
            {
                Body = dto.Body,
                UserId = userId,
                QuestionId = dto.QuestionId,
                AnswerId = dto.AnswerId,
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepository.AddAsync(comment);
            await _commentRepository.SaveChangesAsync();

            _logger.LogInformation("Comment {CommentId} created by user {UserId}", comment.Id, userId);
            return (true, "Comment posted successfully", comment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating comment");
            return (false, "An error occurred while posting the comment", null);
        }
    }

    public async Task<List<Comment>> GetCommentsByQuestionIdAsync(int questionId)
    {
        try
        {
            return await _commentRepository.GetCommentsByQuestionIdAsync(questionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comments for question {QuestionId}", questionId);
            return new List<Comment>();
        }
    }

    public async Task<List<Comment>> GetCommentsByAnswerIdAsync(int answerId)
    {
        try
        {
            return await _commentRepository.GetCommentsByAnswerIdAsync(answerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comments for answer {AnswerId}", answerId);
            return new List<Comment>();
        }
    }

    public async Task<(bool Success, string Message)> UpdateCommentAsync(int commentId, string body, int userId)
    {
        try
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
            {
                return (false, "Comment not found");
            }

            if (comment.UserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to update comment {CommentId} owned by user {OwnerId}", 
                    userId, commentId, comment.UserId);
                return (false, "You do not have permission to update this comment");
            }

            comment.Body = body;

            _commentRepository.Update(comment);
            await _commentRepository.SaveChangesAsync();

            _logger.LogInformation("Comment {CommentId} updated by user {UserId}", commentId, userId);
            return (true, "Comment updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating comment {CommentId}", commentId);
            return (false, "An error occurred while updating the comment");
        }
    }

    public async Task<(bool Success, string Message)> DeleteCommentAsync(int commentId, int userId)
    {
        try
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
            {
                return (false, "Comment not found");
            }

            if (comment.UserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to delete comment {CommentId} owned by user {OwnerId}", 
                    userId, commentId, comment.UserId);
                return (false, "You do not have permission to delete this comment");
            }

            _commentRepository.Remove(comment);
            await _commentRepository.SaveChangesAsync();

            _logger.LogInformation("Comment {CommentId} deleted by user {UserId}", commentId, userId);
            return (true, "Comment deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
            return (false, "An error occurred while deleting the comment");
        }
    }
}
