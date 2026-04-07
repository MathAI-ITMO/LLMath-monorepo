using MathLLMBackend.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MathLLMBackend.Core.Contexts;

public interface IAppDbContext
{
    DbSet<Chat> Chats { get; }
    DbSet<Message> Messages { get; }
    DbSet<UserTask> UserTasks { get; }
    DbSet<Problem> Problems { get; }
    DbSet<GeolinProblemData> GeolinProblems { get; }
    DbSet<ProblemTaskType> ProblemTaskTypes { get; }
    DbSet<ApplicationUser> Users { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
}
