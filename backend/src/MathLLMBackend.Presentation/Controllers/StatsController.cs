using System.Linq;
using System.Threading.Tasks;
using MathLLMBackend.DataAccess.Contexts;
using MathLLMBackend.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MathLLMBackend.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    public StatsController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet("task-mode-titles")]
    public IActionResult GetTaskModeTitles()
    {
        var taskModeTitles = _configuration.GetSection("TaskModeTitles").Get<Dictionary<string, string>>();
        if (taskModeTitles == null)
        {
            return NotFound("TaskModeTitles not found in configuration.");
        }
        return Ok(taskModeTitles);
    }

    [HttpGet("user-stats")]
    public async Task<IActionResult> GetUserStats()
    {
        var stats = await _context.Users
            .Select(u => new UserStatsDto
            {
                UserId = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email ?? string.Empty,
                StudentGroup = u.StudentGroup,
                SolvedCount = _context.UserTasks.Count(ut => ut.ApplicationUserId == u.Id && ut.Status == UserTaskStatus.Solved),
                InProgressCount = _context.UserTasks.Count(ut => ut.ApplicationUserId == u.Id && ut.Status == UserTaskStatus.InProgress),
                NormalChatsCount = _context.Chats.Count(c => c.UserId == u.Id && c.Type == ChatType.Chat)
            })
            .ToListAsync();

        return Ok(stats);
    }

    [HttpGet("user-details/{userId}")]
    public async Task<IActionResult> GetUserDetails(string userId)
    {
        var solvedTasks = await _context.UserTasks
            .Where(ut => ut.ApplicationUserId == userId && ut.Status == UserTaskStatus.Solved)
            .Select(ut => new TaskItemDto
            {
                UserTaskId = ut.Id,
                DisplayName = ut.DisplayName,
                ChatId = ut.AssociatedChatId,
                TaskType = ut.TaskType
            }).ToListAsync();
        var inProgressTasks = await _context.UserTasks
            .Where(ut => ut.ApplicationUserId == userId && ut.Status == UserTaskStatus.InProgress)
            .Select(ut => new TaskItemDto
            {
                UserTaskId = ut.Id,
                DisplayName = ut.DisplayName,
                ChatId = ut.AssociatedChatId,
                TaskType = ut.TaskType
            }).ToListAsync();
        var chats = await _context.Chats
            .Where(c => c.UserId == userId && c.Type == ChatType.Chat)
            .Select(c => new ChatItemDto
            {
                ChatId = c.Id,
                Name = c.Name
            }).ToListAsync();
        var detail = new UserDetailDto
        {
            SolvedTasks = solvedTasks,
            InProgressTasks = inProgressTasks,
            Chats = chats
        };
        return Ok(detail);
    }

    public class UserStatsDto
    {
        public string UserId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string StudentGroup { get; set; } = string.Empty;
        public int SolvedCount { get; set; }
        public int InProgressCount { get; set; }
        public int NormalChatsCount { get; set; }
    }

    public class TaskItemDto
    {
        public Guid UserTaskId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public Guid? ChatId { get; set; }
        public int TaskType { get; set; }
    }

    public class ChatItemDto
    {
        public Guid ChatId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class UserDetailDto
    {
        public List<TaskItemDto> SolvedTasks { get; set; } = new();
        public List<TaskItemDto> InProgressTasks { get; set; } = new();
        public List<ChatItemDto> Chats { get; set; } = new();
    }
} 