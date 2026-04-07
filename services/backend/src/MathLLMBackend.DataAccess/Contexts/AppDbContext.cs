using MathLLMBackend.Core.Contexts;
using MathLLMBackend.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MathLLMBackend.DataAccess.Contexts;

public class AppDbContext : IdentityDbContext<ApplicationUser>, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<UserTask> UserTasks => Set<UserTask>();
    public DbSet<Problem> Problems => Set<Problem>();
    public DbSet<GeolinProblemData> GeolinProblems => Set<GeolinProblemData>();
    public DbSet<ProblemTaskType> ProblemTaskTypes => Set<ProblemTaskType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Chat>(ConfigureChat);
        modelBuilder.Entity<Message>(ConfigureMessage);
        modelBuilder.Entity<ApplicationUser>(ConfigureApplicationUser);
        modelBuilder.Entity<UserTask>(ConfigureUserTask);
        modelBuilder.Entity<Problem>(ConfigureProblem);
        modelBuilder.Entity<GeolinProblemData>(ConfigureGeolinProblems);
        modelBuilder.Entity<ProblemTaskType>(ConfigureProblemTaskType);
    }
    
    private void ConfigureProblemTaskType(EntityTypeBuilder<ProblemTaskType> type)
    {
        type.HasKey(t => new { t.ProblemId, t.TaskType });
        
        type.HasOne(t => t.Problem)
            .WithMany(p => p.Types)
            .HasForeignKey(t => t.ProblemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
    
    private void ConfigureGeolinProblems(EntityTypeBuilder<GeolinProblemData> problem)
    {
        problem.HasKey(p => p.Id);
        problem.Property(p => p.ProblemId).IsRequired();
        problem.Property(p => p.Seed).IsRequired();
        problem.Property(p => p.Hash).IsRequired();
        
        problem.HasIndex(p => new { p.Seed, p.Hash });
        problem.HasIndex(p => p.ProblemId).IsUnique();
    }

    private void ConfigureProblem(EntityTypeBuilder<Problem> problem)
    {
        problem.HasKey(p => p.Id);
        problem.Property(p => p.LlmSolution).IsRequired();
        problem.Property(p => p.Statement).IsRequired();
        problem.Property(p => p.Title).IsRequired();
        problem.Property(p => p.TheoryLink).IsRequired(false);
        problem.HasMany(p => p.Types)
            .WithOne(p => p.Problem)
            .OnDelete(DeleteBehavior.Cascade);

        problem.HasOne(p => p.GeolinProblemData)
            .WithOne()
            .HasForeignKey<GeolinProblemData>(g => g.ProblemId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void ConfigureUserTask(EntityTypeBuilder<UserTask> task)
    {
        task.HasKey(ut => ut.Id);

        task.Property(ut => ut.ApplicationUserId).IsRequired();
        task.Property(ut => ut.ProblemId).IsRequired();
        task.Property(ut => ut.DisplayName).IsRequired().HasMaxLength(1024);
        task.Property(ut => ut.TaskType).IsRequired();
        task.Property(ut => ut.Status).IsRequired();
        task.Property(ut => ut.AssociatedChatId);
        task.Property(ut => ut.ProblemHash).IsRequired();
        
        task.HasOne(ut => ut.Problem)
            .WithMany()
            .HasForeignKey(ut => ut.ProblemId)
            .OnDelete(DeleteBehavior.Cascade);

        task.HasOne(ut => ut.ApplicationUser)
            .WithMany()
            .HasForeignKey(ut => ut.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        task.HasOne(ut => ut.ProblemTaskType)
            .WithMany()
            .HasForeignKey(ut => new { ut.ProblemId, ut.TaskType })
            .HasPrincipalKey(pt => new { pt.ProblemId, pt.TaskType })
            .OnDelete(DeleteBehavior.NoAction);

        task.HasIndex(ut => ut.ApplicationUserId);
        task.HasIndex(ut => new { ut.ApplicationUserId, ut.TaskType });
        task.HasIndex(ut => ut.ProblemId);
        task.HasIndex(ut => ut.AssociatedChatId).IsUnique(false);
    }

    private void ConfigureApplicationUser(EntityTypeBuilder<ApplicationUser> user)
    {
        user
            .Property(u => u.FirstName)
            .HasMaxLength(100);
        
        user
            .Property(u => u.LastName)
            .HasMaxLength(100);
            
        user
            .Property(u => u.StudentGroup)
            .HasMaxLength(20);
    }

    private void ConfigureChat(EntityTypeBuilder<Chat> chat)
    {
        chat
            .HasMany(c => c.Messages)
            .WithOne(m => m.Chat)
            .HasForeignKey(m => m.ChatId);

        chat
            .HasKey(c => c.Id);

        chat
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId);

        chat
            .Property(c => c.UserId)
            .IsRequired();

        chat
            .Property(c => c.Name)
            .IsRequired();

        chat
            .Property(c => c.Type)
            .IsRequired();
    }

    private void ConfigureMessage(EntityTypeBuilder<Message> message)
    {
        message
            .Property(m => m.CreatedAt)
            .IsRequired();

        message
            .Property(m => m.MessageType)
            .IsRequired();

        message
            .Property(m => m.ChatId)
            .IsRequired();

        message
            .Property(m => m.Text)
            .IsRequired();

        message
            .Property(m => m.IsSystemPrompt)
            .IsRequired();
    }
}