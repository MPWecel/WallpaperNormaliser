using System.Data;
using Microsoft.Data.Sqlite;

namespace WallpaperNormaliser.Infrastructure.Persistence.Database;
public sealed class SqliteConnectionFactory
{
    private readonly string _connectionString;
    private readonly string _pragmaSql;

    public SqliteConnectionFactory(string connectionString, string pragmaSql)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _pragmaSql        = pragmaSql        ?? throw new ArgumentNullException(nameof(pragmaSql));
    }

    public IDbConnection Create()
    {
        SqliteConnection connection = new(_connectionString);
        connection.Open();
        EnablePragmas(connection);
        return connection;
    }

    private void EnablePragmas(SqliteConnection conn)
    {
        using SqliteCommand command = conn.CreateCommand();
        command.CommandText = _pragmaSql;
        command.CommandType = CommandType.Text;
        command.ExecuteNonQuery();
    }
}
