using System.Data;
using Npgsql;

namespace MathLLMBackend.Infrastructure
{
    public class DataContext
    {
        private readonly string _connectionString;

        public DataContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}