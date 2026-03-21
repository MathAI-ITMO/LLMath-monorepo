using MathLLMBackend.Domain.Enums;
using MathLLMBackend.Domain.Models;

namespace MathLLMBackend.Core.Services.StatsService;

public interface IStatsService
{
    Task<IEnumerable<UserStats>> GetUserStatsAsync(CancellationToken ct = default);
    Task<UserDetail> GetUserDetailsAsync(string userId, CancellationToken ct = default);
}
