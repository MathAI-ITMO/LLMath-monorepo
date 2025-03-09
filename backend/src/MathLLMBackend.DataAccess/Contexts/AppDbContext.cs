using MathLLMBackend.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MathLLMBackend.DataAccess.Contexts;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext(options)
{
    public DbSet<Chat> Chats { get; set; }
    public DbSet<Message> Messages { get; set; }
}