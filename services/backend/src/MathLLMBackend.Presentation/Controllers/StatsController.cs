using MathLLMBackend.Core.Constants;
using MathLLMBackend.Core.Services.StatsService;
using MathLLMBackend.Domain.Constants;
using MathLLMBackend.Presentation.Binders;
using MathLLMBackend.Presentation.Dtos.Stats;
using MathLLMBackend.Presentation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MathLLMBackend.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatsController : ControllerBase
{
    private readonly IStatsService _statsService;

    public StatsController(IStatsService statsService)
    {
        _statsService = statsService;
    }

    [HttpGet("task-mode-titles")]
    [Authorize(Roles = Role.Admin)]
    public IActionResult GetTaskModeTitles()
    {
        var result = TaskModeTitles.Titles;
        return Ok(result);
    }

    [HttpGet("user-stats")]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> GetUserStats(CancellationToken ct = default)
    {
        var stats = await _statsService.GetUserStatsAsync(ct);

        var dtos = stats.Select(s => new UserStatsDto
        {
            UserId = s.UserId,
            FirstName = s.FirstName,
            LastName = s.LastName,
            Email = s.Email,
            StudentGroup = s.StudentGroup,
            SolvedCount = s.SolvedCount,
            InProgressCount = s.InProgressCount,
            NormalChatsCount = s.NormalChatsCount
        });

        return Ok(dtos);
    }

    [HttpGet("user-details/{userId}")]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> GetUserDetails(string userId, [FromJwt] JwtUser currentUser, CancellationToken ct = default)
    {

        var detail = await _statsService.GetUserDetailsAsync(userId, ct);

        var dto = new UserDetailDto
        {
            SolvedTasks = detail.SolvedTasks.Select(t => new TaskItemDto
            {
                UserTaskId = t.UserTaskId,
                DisplayName = t.DisplayName,
                ChatId = t.ChatId,
                TaskType = t.TaskType
            }).ToList(),
            InProgressTasks = detail.InProgressTasks.Select(t => new TaskItemDto
            {
                UserTaskId = t.UserTaskId,
                DisplayName = t.DisplayName,
                ChatId = t.ChatId,
                TaskType = t.TaskType
            }).ToList(),
            Chats = detail.Chats.Select(c => new ChatItemDto
            {
                ChatId = c.ChatId,
                Name = c.Name
            }).ToList()
        };

        return Ok(dto);
    }
}