using System.Data;
using Microsoft.Data.Sqlite;

namespace WallpaperNormaliser.Infrastructure.Persistence.Database;
public sealed class SqliteConnectionFactory
{
    private readonly string _connectionString;
    public SqliteConnectionFactory(string connString) => _connectionString = connString;

    public IDbConnection Create() => new SqliteConnection(_connectionString);
}
