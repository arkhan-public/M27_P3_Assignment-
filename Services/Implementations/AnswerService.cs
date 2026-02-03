using Microsoft.EntityFrameworkCore;
using QAWebApp.Data;
using QAWebApp.DTOs;
using QAWebApp.Models;
using QAWebApp.Services.Interfaces;

namespace QAWebApp.Services.Implementations;

public class AnswerService : IAnswerService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AnswerService> _logger;

    public AnswerService(ApplicationDbContext context, ILogger<AnswerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, Answer? Answer)> CreateAnswerAsync(AnswerCreateDto dto, int userId)
    {
        try
        {
            var question = await _context.Questions.FindAsync(dto.QuestionId);
            if (question == null)
            {
                return (false, "Question not found", null);
            }

            var answer = new Answer
            {
                Body = dto.Body,
                QuestionId = dto.QuestionId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Answer {AnswerId} created for question {QuestionId} by user {UserId}",
                answer.Id, dto.QuestionId, userId);
            return (true, "Answer posted successfully", answer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating answer for question {QuestionId}", dto.QuestionId);
            return (false, "An error occurred while posting the answer", null);
        }
    }

    public async Task<(bool Success, string Message)> UpdateAnswerAsync(int answerId, AnswerUpdateDto dto, int userId)
    {
        try
        {
            var answer = await _context.Answers.FindAsync(answerId);

            if (answer == null)
            {
                return (false, "Answer not found");
            }

            if (answer.UserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to update answer {AnswerId} owned by user {OwnerId}",
                    userId, answerId, answer.UserId);
                return (false, "You do not have permission to update this answer");
            }

            answer.Body = dto.Body;
            answer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Answer {AnswerId} updated by user {UserId}", answerId, userId);
            return (true, "Answer updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating answer {AnswerId}", answerId);
            return (false, "An error occurred while updating the answer");
        }
    }

    public async Task<(bool Success, string Message)> DeleteAnswerAsync(int answerId, int userId)
    {
        try
        {
            var answer = await _context.Answers.FindAsync(answerId);

            if (answer == null)
            {
                return (false, "Answer not found");
            }

            if (answer.UserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to delete answer {AnswerId} owned by user {OwnerId}",
                    userId, answerId, answer.UserId);
                return (false, "You do not have permission to delete this answer");
            }

            _context.Answers.Remove(answer);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Answer {AnswerId} deleted by user {UserId}", answerId, userId);
            return (true, "Answer deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting answer {AnswerId}", answerId);
            return (false, "An error occurred while deleting the answer");
        }
    }

    public async Task<(bool Success, string Message)> AcceptAnswerAsync(int answerId, int userId)
    {
        try
        {
            var answer = await _context.Answers
                .Include(a => a.Question)
                .FirstOrDefaultAsync(a => a.Id == answerId);

            if (answer == null)
            {
                return (false, "Answer not found");
            }

            if (answer.Question.UserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to accept answer {AnswerId} for question owned by user {OwnerId}",
                    userId, answerId, answer.Question.UserId);
                return (false, "Only the question owner can accept answers");
            }

            // Unaccept any previously accepted answer
            var previouslyAccepted = await _context.Answers
                .Where(a => a.QuestionId == answer.QuestionId && a.IsAccepted)
                .ToListAsync();

            foreach (var prev in previouslyAccepted)
            {
                prev.IsAccepted = false;
            }

            answer.IsAccepted = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Answer {AnswerId} accepted for question {QuestionId}", answerId, answer.QuestionId);
            return (true, "Answer accepted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting answer {AnswerId}", answerId);
            return (false, "An error occurred while accepting the answer");
        }
    }

    public async Task<Answer?> GetAnswerByIdAsync(int answerId)
    {
        try
        {
            return await _context.Answers
                .Include(a => a.User)
                .Include(a => a.Question)
                .FirstOrDefaultAsync(a => a.Id == answerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving answer {AnswerId}", answerId);
            return null;
        }
    }

    public async Task<List<Answer>> GetAnswersByQuestionIdAsync(int questionId)
    {
        try
        {
            return await _context.Answers
                .Include(a => a.User)
                .Where(a => a.QuestionId == questionId)
                .OrderByDescending(a => a.IsAccepted)
                .ThenByDescending(a => a.VoteCount)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving answers for question {QuestionId}", questionId);
            return new List<Answer>();
        }
    }
}
