namespace MathLLMBackend.DataAccess.Configuration;

public class DatabaseConfiguration
{
    public const string SectionName = "Database";
    
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.Postgres;
    public string? InMemoryDatabaseName { get; set; }
}
