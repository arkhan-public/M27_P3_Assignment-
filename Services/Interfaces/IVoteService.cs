using QAWebApp.DTOs;

namespace QAWebApp.Services.Interfaces;

public interface IVoteService
{
    Task<(bool Success, string Message)> VoteAsync(VoteDto dto, int userId);
    Task<int> GetVoteCountAsync(int? questionId, int? answerId);
    Task<bool> HasUserVotedAsync(int userId, int? questionId, int? answerId);
}
