using MathLLMBackend.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MathLLMBackend.DataAccess.Contexts;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext(options)
{
    public DbSet<Chat> Chats { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chat>()
            .HasMany(c => c.Messages)
            .WithOne(m => m.Chat)
            .HasForeignKey(m => m.ChatId);

        modelBuilder.Entity<Chat>()
            .HasKey(c => c.Id);
        
        modelBuilder.Entity<Chat>()
            .HasOne(c => c.User)
            .WithOne()
            .HasForeignKey<Chat>(c => c.UserId);

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
        
        base.OnModelCreating(modelBuilder);
    }
}