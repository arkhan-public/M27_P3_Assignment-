using QAWebApp.DTOs;
using QAWebApp.Models;

namespace QAWebApp.Services.Interfaces;

public interface IVoteService
{
    Task<(bool Success, string Message)> VoteAsync(VoteDto dto, int userId);
    Task<int> GetVoteCountAsync(int? questionId, int? answerId);
    Task<bool> HasUserVotedAsync(int userId, int? questionId, int? answerId);
    Task<VoteType?> GetUserVoteTypeAsync(int userId, int? questionId, int? answerId);
}
