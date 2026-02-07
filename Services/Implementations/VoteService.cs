using QAWebApp.DTOs;
using QAWebApp.Models;
using QAWebApp.Repositories.Interfaces;
using QAWebApp.Services.Interfaces;

namespace QAWebApp.Services.Implementations;

public class VoteService : IVoteService
{
    private readonly IVoteRepository _voteRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IAnswerRepository _answerRepository;
    private readonly ILogger<VoteService> _logger;

    public VoteService(
        IVoteRepository voteRepository,
        IQuestionRepository questionRepository,
        IAnswerRepository answerRepository,
        ILogger<VoteService> logger)
    {
        _voteRepository = voteRepository;
        _questionRepository = questionRepository;
        _answerRepository = answerRepository;
        _logger = logger;
    }

    public async Task<(bool Success, string Message)> VoteAsync(VoteDto dto, int userId)
    {
        try
        {
            if (dto.QuestionId == null && dto.AnswerId == null)
            {
                return (false, "Vote must be associated with a question or answer");
            }

            // Check if user is trying to vote on their own question
            if (dto.QuestionId.HasValue)
            {
                var question = await _questionRepository.GetByIdAsync(dto.QuestionId.Value);
                if (question != null && question.UserId == userId)
                {
                    _logger.LogWarning("User {UserId} attempted to vote on their own question {QuestionId}", 
                        userId, dto.QuestionId.Value);
                    return (false, "You cannot vote on your own question");
                }
            }

            // Check if user is trying to vote on their own answer
            if (dto.AnswerId.HasValue)
            {
                var answer = await _answerRepository.GetByIdAsync(dto.AnswerId.Value);
                if (answer != null && answer.UserId == userId)
                {
                    _logger.LogWarning("User {UserId} attempted to vote on their own answer {AnswerId}", 
                        userId, dto.AnswerId.Value);
                    return (false, "You cannot vote on your own answer");
                }
            }

            // Check if user already voted
            var existingVote = await _voteRepository.GetVoteAsync(userId, dto.QuestionId, dto.AnswerId);

            if (existingVote != null)
            {
                // If same vote type, remove the vote
                if (existingVote.Type == dto.Type)
                {
                    _voteRepository.Remove(existingVote);
                    await UpdateVoteCount(dto.QuestionId, dto.AnswerId, -(int)existingVote.Type);
                    _logger.LogInformation("Vote removed by user {UserId}. Vote count adjusted by {Delta}", 
                        userId, -(int)existingVote.Type);
                    return (true, "Vote removed");
                }
                else
                {
                    // Change vote type
                    var oldVoteValue = (int)existingVote.Type;
                    existingVote.Type = dto.Type;
                    _voteRepository.Update(existingVote);
                    
                    // Calculate the delta: new vote - old vote
                    // Example: changing from -1 to +1 = +1 - (-1) = +2
                    var delta = (int)dto.Type - oldVoteValue;
                    await UpdateVoteCount(dto.QuestionId, dto.AnswerId, delta);
                    
                    _logger.LogInformation("Vote changed by user {UserId} from {OldVote} to {NewVote}. Delta: {Delta}", 
                        userId, oldVoteValue, (int)dto.Type, delta);
                    return (true, "Vote changed successfully");
                }
            }

            // Create new vote
            var vote = new Vote
            {
                Type = dto.Type,
                UserId = userId,
                QuestionId = dto.QuestionId,
                AnswerId = dto.AnswerId,
                CreatedAt = DateTime.UtcNow
            };

            await _voteRepository.AddAsync(vote);
            await UpdateVoteCount(dto.QuestionId, dto.AnswerId, (int)dto.Type);

            _logger.LogInformation("Vote created by user {UserId}. Type: {VoteType}, Delta: {Delta}", 
                userId, dto.Type, (int)dto.Type);
            return (true, "Vote recorded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing vote");
            return (false, "An error occurred while processing the vote");
        }
    }

    public async Task<int> GetVoteCountAsync(int? questionId, int? answerId)
    {
        try
        {
            if (questionId.HasValue)
            {
                return await _voteRepository.GetQuestionVoteCountAsync(questionId.Value);
            }

            if (answerId.HasValue)
            {
                return await _voteRepository.GetAnswerVoteCountAsync(answerId.Value);
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vote count");
            return 0;
        }
    }

    public async Task<bool> HasUserVotedAsync(int userId, int? questionId, int? answerId)
    {
        try
        {
            var vote = await _voteRepository.GetVoteAsync(userId, questionId, answerId);
            return vote != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user vote");
            return false;
        }
    }

    public async Task<VoteType?> GetUserVoteTypeAsync(int userId, int? questionId, int? answerId)
    {
        try
        {
            var vote = await _voteRepository.GetVoteAsync(userId, questionId, answerId);
            return vote?.Type;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user vote type");
            return null;
        }
    }

    private async Task UpdateVoteCount(int? questionId, int? answerId, int delta)
    {
        if (questionId.HasValue)
        {
            var question = await _questionRepository.GetByIdAsync(questionId.Value);
            if (question != null)
            {
                question.VoteCount += delta;
                _questionRepository.Update(question);
                await _questionRepository.SaveChangesAsync();
            }
        }

        if (answerId.HasValue)
        {
            var answer = await _answerRepository.GetByIdAsync(answerId.Value);
            if (answer != null)
            {
                answer.VoteCount += delta;
                _answerRepository.Update(answer);
                await _answerRepository.SaveChangesAsync();
            }
        }
    }
}
