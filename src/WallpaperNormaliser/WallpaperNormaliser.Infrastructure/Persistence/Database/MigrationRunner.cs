using System.Data;

using Dapper;

namespace WallpaperNormaliser.Infrastructure.Persistence.Database;
public sealed class MigrationRunner
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public MigrationRunner(SqliteConnectionFactory connectionFactory) 
        => _connectionFactory = connectionFactory;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        using IDbConnection db = _connectionFactory.Create();

        string sql = await File.ReadAllTextAsync(MigrationConstants._relativeDbScriptPath_Pragmas, cancellationToken);

        await db.ExecuteAsync(sql);
    }

    private async Task<string> LoadEmbeddedSqlAsync(string resourceName, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    private async Task<string> ReadSqlFromFileAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    private async Task EnsureSchemaInfoAsync(IDbConnection connection, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}

internal static class MigrationConstants
{
    internal static readonly string _relativeDbScriptPath_Pragmas = "Persistence/Sql/001_Initial.sql";
}
