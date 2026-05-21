using System.Data;

using Dapper;

namespace WallpaperNormaliser.Infrastructure.Persistence.Database;
public sealed class MigrationRunner(SqliteConnectionFactory connectionFactory)
{
    private const int _expectedSchemaVersion = 1;
    private const string _schemaCreationFolder = "Persistence/Sql/SchemaCreation";

    private readonly SqliteConnectionFactory _connectionFactory = connectionFactory;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        using IDbConnection db = _connectionFactory.Create();

        int? currentVersion = await GetCurrentSchemaVersionAsync(db);
        if (currentVersion == _expectedSchemaVersion)
            return;

        IReadOnlyList<string> sqlFiles = GetOrderedSchemaCreationFiles();

        using IDbTransaction tx = db.BeginTransaction();
        try
        {
            foreach (string filePath in sqlFiles)
            {
                string sql = await ReadSqlFromFileAsync(filePath, cancellationToken);
                await db.ExecuteAsync(sql, transaction: tx);
            }
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    private static async Task<int?> GetCurrentSchemaVersionAsync(IDbConnection db)
    {
        const string tableExistsSql = "SELECT name FROM sqlite_master WHERE type='table' AND name='SchemaInfo';";
        string? tableName = await db.QueryFirstOrDefaultAsync<string?>(tableExistsSql);
        if (string.IsNullOrEmpty(tableName))
            return null;

        const string versionSql = "SELECT MAX([Version]) FROM [SchemaInfo];";
        return await db.QueryFirstOrDefaultAsync<int?>(versionSql);
    }

    private static IReadOnlyList<string> GetOrderedSchemaCreationFiles()
    {
        string folder = Path.Combine(AppContext.BaseDirectory, _schemaCreationFolder);
        return Directory
            .GetFiles(folder, "*.sql", SearchOption.TopDirectoryOnly)
            .OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static Task<string> ReadSqlFromFileAsync(string filePath, CancellationToken cancellationToken)
        => File.ReadAllTextAsync(filePath, cancellationToken);
}
