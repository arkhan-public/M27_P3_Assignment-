using QAWebApp.DTOs;
using QAWebApp.Models;
using QAWebApp.Repositories.Interfaces;
using QAWebApp.Services.Interfaces;

namespace QAWebApp.Services.Implementations;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IAnswerRepository _answerRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IVoteRepository _voteRepository;
    private readonly ILogger<QuestionService> _logger;

    public QuestionService(
        IQuestionRepository questionRepository,
        ITagRepository tagRepository,
        IAnswerRepository answerRepository,
        ICommentRepository commentRepository,
        IVoteRepository voteRepository,
        ILogger<QuestionService> logger)
    {
        _questionRepository = questionRepository;
        _tagRepository = tagRepository;
        _answerRepository = answerRepository;
        _commentRepository = commentRepository;
        _voteRepository = voteRepository;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, Question? Question)> CreateQuestionAsync(QuestionCreateDto dto, int userId)
    {
        try
        {
            // Get or create tags
            var tagNames = dto.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(t => t.Trim().ToLower())
                                   .Distinct()
                                   .ToList();

            var existingTags = await _tagRepository.GetTagsByNamesAsync(tagNames);
            var tags = new List<Tag>();

            foreach (var tagName in tagNames)
            {
                var tag = existingTags.FirstOrDefault(t => t.Name.ToLower() == tagName);
                if (tag == null)
                {
                    tag = new Tag { Name = tagName, CreatedAt = DateTime.UtcNow };
                    await _tagRepository.AddAsync(tag);
                    await _tagRepository.SaveChangesAsync();
                    _logger.LogInformation("New tag created: {TagName}", tagName);
                }
                tags.Add(tag);
            }

            // Create question
            var question = new Question
            {
                Title = dto.Title,
                Body = dto.Body,
                UserId = userId,
                Tags = tags,
                CreatedAt = DateTime.UtcNow
            };

            await _questionRepository.AddAsync(question);
            await _questionRepository.SaveChangesAsync();

            _logger.LogInformation("Question {QuestionId} created by user {UserId}", question.Id, userId);
            return (true, "Question created successfully", question);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating question");
            return (false, $"Error creating question: {ex.Message}", null);
        }
    }

    public async Task<(bool Success, string Message)> UpdateQuestionAsync(int questionId, QuestionUpdateDto dto, int userId)
    {
        try
        {
            // Get the question with its tags included
            var question = await _questionRepository.GetQuestionWithDetailsAsync(questionId);
            
            if (question == null)
                return (false, "Question not found");

            if (question.UserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to update question {QuestionId} owned by user {OwnerId}",
                    userId, questionId, question.UserId);
                return (false, "Unauthorized to update this question");
            }

            // Update basic fields
            question.Title = dto.Title;
            question.Body = dto.Body;
            question.UpdatedAt = DateTime.UtcNow;

            // Process tags
            var tagNames = dto.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(t => t.Trim().ToLower())
                                   .Distinct()
                                   .ToList();

            // Get or create tags
            var existingTags = await _tagRepository.GetTagsByNamesAsync(tagNames);
            var updatedTags = new List<Tag>();

            foreach (var tagName in tagNames)
            {
                var tag = existingTags.FirstOrDefault(t => t.Name.ToLower() == tagName);
                if (tag == null)
                {
                    // Create new tag if it doesn't exist
                    tag = new Tag { Name = tagName, CreatedAt = DateTime.UtcNow };
                    await _tagRepository.AddAsync(tag);
                    await _tagRepository.SaveChangesAsync();
                    _logger.LogInformation("New tag created during question update: {TagName}", tagName);
                }
                updatedTags.Add(tag);
            }

            // Clear existing tags and add new ones
            question.Tags.Clear();
            foreach (var tag in updatedTags)
            {
                question.Tags.Add(tag);
            }

            _questionRepository.Update(question);
            await _questionRepository.SaveChangesAsync();

            _logger.LogInformation("Question {QuestionId} updated by user {UserId} with {TagCount} tags", 
                questionId, userId, updatedTags.Count);
            return (true, "Question updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating question {QuestionId}", questionId);
            return (false, $"Error updating question: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> DeleteQuestionAsync(int questionId, int userId)
    {
        try
        {
            // Load question with all related data
            var question = await _questionRepository.GetQuestionWithDetailsAsync(questionId);
            
            if (question == null)
                return (false, "Question not found");

            if (question.UserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to delete question {QuestionId} owned by user {OwnerId}",
                    userId, questionId, question.UserId);
                return (false, "Unauthorized to delete this question");
            }

            // Step 1: Delete all votes on the question
            var questionVotes = await _voteRepository.GetVotesByQuestionIdAsync(questionId);
            if (questionVotes.Any())
            {
                _voteRepository.RemoveRange(questionVotes);
                await _voteRepository.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} votes from question {QuestionId}", 
                    questionVotes.Count, questionId);
            }

            // Step 2: Delete all answer-related data
            foreach (var answer in question.Answers)
            {
                // Step 2a: Delete comments on this answer
                var answerComments = await _commentRepository.GetCommentsByAnswerIdAsync(answer.Id);
                if (answerComments.Any())
                {
                    _commentRepository.RemoveRange(answerComments);
                    await _commentRepository.SaveChangesAsync();
                    _logger.LogInformation("Deleted {Count} comments from answer {AnswerId} of question {QuestionId}", 
                        answerComments.Count, answer.Id, questionId);
                }

                // Step 2b: Delete votes on this answer
                var answerVotes = await _voteRepository.GetVotesByAnswerIdAsync(answer.Id);
                if (answerVotes.Any())
                {
                    _voteRepository.RemoveRange(answerVotes);
                    await _voteRepository.SaveChangesAsync();
                    _logger.LogInformation("Deleted {Count} votes from answer {AnswerId} of question {QuestionId}", 
                        answerVotes.Count, answer.Id, questionId);
                }
            }

            // Step 3: Delete all answers (after their comments and votes are deleted)
            if (question.Answers.Any())
            {
                _answerRepository.RemoveRange(question.Answers.ToList());
                await _answerRepository.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} answers from question {QuestionId}", 
                    question.Answers.Count, questionId);
            }

            // Step 4: Delete all comments on the question
            var questionComments = await _commentRepository.GetCommentsByQuestionIdAsync(questionId);
            if (questionComments.Any())
            {
                _commentRepository.RemoveRange(questionComments);
                await _commentRepository.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} comments from question {QuestionId}", 
                    questionComments.Count, questionId);
            }

            // Step 5: Finally, delete the question
            _questionRepository.Remove(question);
            await _questionRepository.SaveChangesAsync();

            _logger.LogInformation("Question {QuestionId} and all related data deleted successfully by user {UserId}", 
                questionId, userId);
            return (true, "Question and all related content deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting question {QuestionId}", questionId);
            return (false, $"Error deleting question: {ex.Message}");
        }
    }

    public async Task<Question?> GetQuestionByIdAsync(int questionId)
    {
        try
        {
            return await _questionRepository.GetQuestionWithDetailsAsync(questionId);
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
            return await _questionRepository.GetAllQuestionsWithDetailsAsync(searchTerm, tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all questions");
            return new List<Question>();
        }
    }

    public async Task<List<Question>> GetLatestQuestionsAsync(int count = 10)
    {
        try
        {
            return await _questionRepository.GetLatestQuestionsAsync(count);
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
            await _questionRepository.IncrementViewCountAsync(questionId);
            await _questionRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing view count for question {QuestionId}", questionId);
        }
    }
}
