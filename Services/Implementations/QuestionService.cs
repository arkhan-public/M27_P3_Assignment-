using Microsoft.EntityFrameworkCore;
using QAWebApp.Data;
using QAWebApp.DTOs;
using QAWebApp.Models;
using QAWebApp.Services.Interfaces;

namespace QAWebApp.Services.Implementations;

public class QuestionService : IQuestionService
{
    private readonly ApplicationDbContext _context;
    private readonly ITagService _tagService;
    private readonly ILogger<QuestionService> _logger;

    public QuestionService(ApplicationDbContext context, ITagService tagService, ILogger<QuestionService> logger)
    {
        _context = context;
        _tagService = tagService;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, Question? Question)> CreateQuestionAsync(QuestionCreateDto dto, int userId)
    {
        try
        {
            var tags = await _tagService.GetOrCreateTagsAsync(dto.Tags);

            var question = new Question
            {
                Title = dto.Title,
                Body = dto.Body,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Tags = tags
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Question {QuestionId} created by user {UserId}", question.Id, userId);
            return (true, "Question created successfully", question);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating question for user {UserId}", userId);
            return (false, "An error occurred while creating the question", null);
        }
    }

    public async Task<(bool Success, string Message)> UpdateQuestionAsync(int questionId, QuestionUpdateDto dto, int userId)
    {
        try
        {
            var question = await _context.Questions
                .Include(q => q.Tags)
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null)
            {
                return (false, "Question not found");
            }

            if (question.UserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to update question {QuestionId} owned by user {OwnerId}",
                    userId, questionId, question.UserId);
                return (false, "You do not have permission to update this question");
            }

            question.Title = dto.Title;
            question.Body = dto.Body;
            question.UpdatedAt = DateTime.UtcNow;

            // Update tags
            question.Tags.Clear();
            var tags = await _tagService.GetOrCreateTagsAsync(dto.Tags);
            question.Tags = tags;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Question {QuestionId} updated by user {UserId}", questionId, userId);
            return (true, "Question updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating question {QuestionId}", questionId);
            return (false, "An error occurred while updating the question");
        }
    }

    public async Task<(bool Success, string Message)> DeleteQuestionAsync(int questionId, int userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var question = await _context.Questions
                .Include(q => q.Answers)
                    .ThenInclude(a => a.Comments)
                .Include(q => q.Comments)
                .Include(q => q.Votes)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.Votes)
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null)
            {
                await transaction.RollbackAsync();
                return (false, "Question not found");
            }

            if (question.UserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to delete question {QuestionId} owned by user {OwnerId}",
                    userId, questionId, question.UserId);
                await transaction.RollbackAsync();
                return (false, "You do not have permission to delete this question");
            }

            // Step 1: Delete all comments on answers
            var answerComments = question.Answers.SelectMany(a => a.Comments).ToList();
            if (answerComments.Any())
            {
                _context.Comments.RemoveRange(answerComments);
                _logger.LogInformation("Deleting {Count} comments from answers of question {QuestionId}", 
                    answerComments.Count, questionId);
            }

            // Step 2: Delete all votes on answers
            var answerVotes = question.Answers.SelectMany(a => a.Votes).ToList();
            if (answerVotes.Any())
            {
                _context.Votes.RemoveRange(answerVotes);
                _logger.LogInformation("Deleting {Count} votes from answers of question {QuestionId}", 
                    answerVotes.Count, questionId);
            }

            // Step 3: Delete all answers
            if (question.Answers.Any())
            {
                _context.Answers.RemoveRange(question.Answers);
                _logger.LogInformation("Deleting {Count} answers for question {QuestionId}", 
                    question.Answers.Count, questionId);
            }

            // Step 4: Delete all comments on the question
            if (question.Comments.Any())
            {
                _context.Comments.RemoveRange(question.Comments);
                _logger.LogInformation("Deleting {Count} comments from question {QuestionId}", 
                    question.Comments.Count, questionId);
            }

            // Step 5: Delete all votes on the question
            if (question.Votes.Any())
            {
                _context.Votes.RemoveRange(question.Votes);
                _logger.LogInformation("Deleting {Count} votes from question {QuestionId}", 
                    question.Votes.Count, questionId);
            }

            // Step 6: Finally, delete the question
            _context.Questions.Remove(question);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Question {QuestionId} and all related data deleted successfully by user {UserId}", 
                questionId, userId);
            return (true, "Question and all related content deleted successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting question {QuestionId}", questionId);
            return (false, "An error occurred while deleting the question");
        }
    }

    public async Task<Question?> GetQuestionByIdAsync(int questionId)
    {
        try
        {
            return await _context.Questions
                .Include(q => q.User)
                .Include(q => q.Tags)
                .Include(q => q.Answers.OrderByDescending(a => a.IsAccepted).ThenByDescending(a => a.VoteCount))
                    .ThenInclude(a => a.User)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.Comments)
                        .ThenInclude(c => c.User)
                .Include(q => q.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(q => q.Id == questionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving question {QuestionId}", questionId);
            return null;
        }
    }

    public async Task<List<Question>> GetAllQuestionsAsync(string? searchTerm = null, string? tag = null)
    {
        try
        {
            var query = _context.Questions
                .Include(q => q.User)
                .Include(q => q.Tags)
                .Include(q => q.Answers)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(q => q.Title.Contains(searchTerm) || q.Body.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(tag))
            {
                query = query.Where(q => q.Tags.Any(t => t.Name == tag));
            }

            return await query.OrderByDescending(q => q.CreatedAt).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving questions");
            return new List<Question>();
        }
    }

    public async Task<List<Question>> GetLatestQuestionsAsync(int count = 10)
    {
        try
        {
            return await _context.Questions
                .Include(q => q.User)
                .Include(q => q.Tags)
                .Include(q => q.Answers)
                .OrderByDescending(q => q.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving latest questions");
            return new List<Question>();
        }
    }

    public async Task IncrementViewCountAsync(int questionId)
    {
        try
        {
            var question = await _context.Questions.FindAsync(questionId);
            if (question != null)
            {
                question.ViewCount++;
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing view count for question {QuestionId}", questionId);
        }
    }
}
