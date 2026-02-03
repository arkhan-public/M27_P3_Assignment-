using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QAWebApp.DTOs;
using QAWebApp.Services.Interfaces;
using System.Security.Claims;

namespace QAWebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VoteController : ControllerBase
{
    private readonly IVoteService _voteService;
    private readonly ILogger<VoteController> _logger;

    public VoteController(IVoteService voteService, ILogger<VoteController> logger)
    {
        _voteService = voteService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Vote([FromBody] VoteDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim);
        var result = await _voteService.VoteAsync(dto, userId);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = result.Message });
    }
}
