using MathLLMBackend.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MathLLMBackend.DataAccess.Contexts;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<UserTask> UserTasks => Set<UserTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Chat>()
            .HasMany(c => c.Messages)
            .WithOne(m => m.Chat)
            .HasForeignKey(m => m.ChatId);

        modelBuilder.Entity<Chat>()
            .HasKey(c => c.Id);
        
        modelBuilder.Entity<Chat>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId);

        modelBuilder.Entity<Chat>()
            .Property(c => c.UserId)
            .IsRequired();
        
        modelBuilder.Entity<Chat>()
            .Property(c => c.Name)
            .IsRequired();

        modelBuilder.Entity<Message>()
            .Property(m => m.CreatedAt)
            .IsRequired();
        
        modelBuilder.Entity<Message>()
            .Property(m => m.MessageType)
            .IsRequired();
        
        modelBuilder.Entity<Message>()
            .Property(m => m.ChatId)
            .IsRequired();
        
        modelBuilder.Entity<Message>()
            .Property(m => m.Text)
            .IsRequired();

        modelBuilder.Entity<Message>()
            .Property(m => m.IsSystemPrompt)
            .IsRequired();

        modelBuilder.Entity<Chat>()
            .Property(c => c.Type)
            .IsRequired();
            
        // Конфигурация для ApplicationUser
        modelBuilder.Entity<ApplicationUser>()
            .Property(u => u.FirstName)
            .HasMaxLength(100);
            
        modelBuilder.Entity<ApplicationUser>()
            .Property(u => u.LastName)
            .HasMaxLength(100);
            
        modelBuilder.Entity<ApplicationUser>()
            .Property(u => u.StudentGroup)
            .HasMaxLength(20);
            
        // Конфигурация для UserTask
        modelBuilder.Entity<UserTask>(entity =>
        {
            entity.HasKey(ut => ut.Id);

            entity.Property(ut => ut.ApplicationUserId).IsRequired();
            entity.Property(ut => ut.ProblemId).IsRequired().HasMaxLength(512);
            entity.Property(ut => ut.DisplayName).IsRequired().HasMaxLength(1024);
            entity.Property(ut => ut.TaskType).IsRequired();
            entity.Property(ut => ut.Status).IsRequired();
            entity.Property(ut => ut.AssociatedChatId); // Nullable по умолчанию

            // Связь с ApplicationUser
            entity.HasOne(ut => ut.ApplicationUser)
                  .WithMany() // У пользователя может быть много задач
                  .HasForeignKey(ut => ut.ApplicationUserId)
                  .OnDelete(DeleteBehavior.Cascade); // Удалять задачи пользователя при удалении пользователя

            // Индексы для ускорения запросов
            entity.HasIndex(ut => ut.ApplicationUserId);
            entity.HasIndex(ut => new { ut.ApplicationUserId, ut.TaskType });
            entity.HasIndex(ut => ut.ProblemId);
            entity.HasIndex(ut => ut.AssociatedChatId).IsUnique(false); // Может быть null или повторяться, если переделывать задачи?
                                                                         // Если чат уникален для задачи, то .IsUnique() - но AssociatedChatId nullable.
                                                                         // Пока оставим неуникальный индекс.
        });
    }
}