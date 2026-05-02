using System.Data;
using Microsoft.Data.Sqlite;

namespace WallpaperNormaliser.Infrastructure.Persistence.Database;
public sealed class SqliteConnectionFactory
{
    private readonly string _connectionString;
    public SqliteConnectionFactory(string connectionString) 
        => _connectionString = connectionString 
                                ?? throw new ArgumentNullException(nameof(connectionString));

    public IDbConnection Create()
    {
        SqliteConnection connection = new(_connectionString);
        connection.Open();
        EnablePragmas(connection);
        return connection;
    }

    private static void EnablePragmas(SqliteConnection conn)
    {
        string commandString = """
                                  PRAGMA foreign_keys = ON;
                                  PRAGMA journal_mode = WAL;
                                  PRAGMA synchronous = NORMAL;
                                  PRAGMA temp_store = MEMORY;
                                  PRAGMA cache_size = -20000;
                               """;
        using SqliteCommand command = conn.CreateCommand();
        command.CommandText = commandString;
        command.CommandType = CommandType.Text;
        command.ExecuteNonQuery();
    }
}
