using MathLLMBackend.Core.Constants;
using MathLLMBackend.Domain.Models;
using MathLLMBackend.Core.Contexts;
using MathLLMBackend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MathLLMBackend.Core.Services.StatsService;

public class StatsService : IStatsService
{
    private readonly IAppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StatsService> _logger;

    public StatsService(
        IAppDbContext context,
        IConfiguration configuration,
        ILogger<StatsService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IEnumerable<UserStats>> GetUserStatsAsync(CancellationToken ct = default)
    {
        var stats = await _context.Users
            .Select(u => new UserStats
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
            .ToListAsync(ct);

        return stats;
    }

    public async Task<UserDetail> GetUserDetailsAsync(string userId, CancellationToken ct = default)
    {
        var solvedTasks = await _context.UserTasks
            .Where(ut => ut.ApplicationUserId == userId && ut.Status == UserTaskStatus.Solved)
            .Select(ut => new TaskItem
            {
                UserTaskId = ut.Id,
                DisplayName = ut.DisplayName,
                ChatId = ut.AssociatedChatId,
                TaskType = ut.ProblemTaskType.TaskType
            })
            .ToListAsync(ct);

        var inProgressTasks = await _context.UserTasks
            .Where(ut => ut.ApplicationUserId == userId && ut.Status == UserTaskStatus.InProgress)
            .Select(ut => new TaskItem
            {
                UserTaskId = ut.Id,
                DisplayName = ut.DisplayName,
                ChatId = ut.AssociatedChatId,
                TaskType = ut.ProblemTaskType.TaskType
            })
            .ToListAsync(ct);

        var chats = await _context.Chats
            .Where(c => c.UserId == userId && c.Type == ChatType.Chat)
            .Select(c => new ChatItem
            {
                ChatId = c.Id,
                Name = c.Name
            })
            .ToListAsync(ct);

        return new UserDetail
        {
            SolvedTasks = solvedTasks,
            InProgressTasks = inProgressTasks,
            Chats = chats
        };
    }
}
