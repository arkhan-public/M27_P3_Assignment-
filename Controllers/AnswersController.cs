using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QAWebApp.DTOs;
using QAWebApp.Services.Interfaces;
using System.Security.Claims;

namespace QAWebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnswersController : ControllerBase
{
    private readonly IAnswerService _answerService;
    private readonly ILogger<AnswersController> _logger;

    public AnswersController(IAnswerService answerService, ILogger<AnswersController> logger)
    {
        _answerService = answerService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAnswer([FromBody] AnswerCreateDto dto)
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
        var result = await _answerService.CreateAnswerAsync(dto, userId);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = result.Message, answerId = result.Answer!.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAnswer(int id, [FromBody] AnswerUpdateDto dto)
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
        var result = await _answerService.UpdateAnswerAsync(id, dto, userId);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = result.Message });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAnswer(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim);
        var result = await _answerService.DeleteAnswerAsync(id, userId);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = result.Message });
    }

    [HttpPost("{id}/accept")]
    public async Task<IActionResult> AcceptAnswer(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim);
        var result = await _answerService.AcceptAnswerAsync(id, userId);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = result.Message });
    }
}
