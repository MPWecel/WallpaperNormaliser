using System.Data;
using Microsoft.Data.Sqlite;

namespace WallpaperNormaliser.Infrastructure.Persistence.Database;
public sealed class SqliteConnectionFactory
{
    // Every call creates a new SqliteConnection, but Microsoft.Data.Sqlite pools
    // the underlying sqlite3* handle by connection string — so after the first open,
    // each Create() amounts to a pool grab, not a real handle allocation.
    //
    // PRAGMAs are re-run on every open. WAL is database-level and persists across
    // connections; the connection-level PRAGMAs (cache_size, temp_store, foreign_keys)
    // survive on the pooled handle but re-applying them is cheap and idempotent.
    //
    // If profiling ever flags this factory as hot, options are:
    //   (a) hold a long-lived IDbConnection here and hand out the same instance
    //       (SQLite + WAL supports concurrent reads on one connection),
    //   (b) apply PRAGMAs once at pool warm-up and skip them on subsequent Create()
    //       (would need a per-handle "already primed" marker; not trivial with the
    //       pooler managing lifetimes).
    // Neither is worth doing until measurement demands it.

    private readonly string _connectionString;
    private readonly string _pragmaSql;

    public SqliteConnectionFactory(string connectionString, string pragmaSql)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _pragmaSql = pragmaSql ?? throw new ArgumentNullException(nameof(pragmaSql));
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
