using Microsoft.EntityFrameworkCore;
using QAWebApp.Data;
using QAWebApp.DTOs;
using QAWebApp.Models;
using QAWebApp.Services.Interfaces;

namespace QAWebApp.Services.Implementations;

public class VoteService : IVoteService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<VoteService> _logger;

    public VoteService(ApplicationDbContext context, ILogger<VoteService> logger)
    {
        _context = context;
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

            // Check if user already voted
            var existingVote = await _context.Votes
                .FirstOrDefaultAsync(v => v.UserId == userId &&
                    v.QuestionId == dto.QuestionId &&
                    v.AnswerId == dto.AnswerId);

            if (existingVote != null)
            {
                // If same vote type, remove the vote
                if (existingVote.Type == dto.Type)
                {
                    _context.Votes.Remove(existingVote);
                    await UpdateVoteCount(dto.QuestionId, dto.AnswerId, -(int)existingVote.Type);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Vote removed by user {UserId}", userId);
                    return (true, "Vote removed");
                }
                else
                {
                    // Change vote type
                    var oldVoteValue = (int)existingVote.Type;
                    existingVote.Type = dto.Type;
                    await UpdateVoteCount(dto.QuestionId, dto.AnswerId, (int)dto.Type - oldVoteValue);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Vote updated by user {UserId}", userId);
                    return (true, "Vote updated");
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

            _context.Votes.Add(vote);
            await UpdateVoteCount(dto.QuestionId, dto.AnswerId, (int)dto.Type);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Vote created by user {UserId}", userId);
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
                var question = await _context.Questions.FindAsync(questionId.Value);
                return question?.VoteCount ?? 0;
            }

            if (answerId.HasValue)
            {
                var answer = await _context.Answers.FindAsync(answerId.Value);
                return answer?.VoteCount ?? 0;
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
            return await _context.Votes
                .AnyAsync(v => v.UserId == userId &&
                    v.QuestionId == questionId &&
                    v.AnswerId == answerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user vote");
            return false;
        }
    }

    private async Task UpdateVoteCount(int? questionId, int? answerId, int delta)
    {
        if (questionId.HasValue)
        {
            var question = await _context.Questions.FindAsync(questionId.Value);
            if (question != null)
            {
                question.VoteCount += delta;
            }
        }

        if (answerId.HasValue)
        {
            var answer = await _context.Answers.FindAsync(answerId.Value);
            if (answer != null)
            {
                answer.VoteCount += delta;
            }
        }
    }
}
