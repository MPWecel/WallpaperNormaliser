using System.Data;
using System.Globalization;

using Dapper;

namespace WallpaperNormaliser.Infrastructure.Persistence.Database;
public sealed class MigrationRunner(SqliteConnectionFactory connectionFactory)
{
    private const int LatestSchemaVersion = 1;
    private const string MigrationsRootFolder = "Persistence/Migrations";

    private readonly SqliteConnectionFactory _connectionFactory = connectionFactory;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConn = _connectionFactory.Create();

        await EnsureSchemaInfoTableAsync(dbConn);

        int currentVersion = await GetCurrentSchemaVersionAsync(dbConn);
        bool isMigrationRunUnnecessary = currentVersion >= LatestSchemaVersion;

        if (isMigrationRunUnnecessary)
            return;

        for (int version = currentVersion + 1; version <= LatestSchemaVersion; version++)
        {
            IReadOnlyList<string> files = GetOrderedMigrationFiles(version);
            bool isFileListEmpty = files.Count == 0;

            if (isFileListEmpty)
                continue;

            using IDbTransaction tx = dbConn.BeginTransaction();
            try
            {
                foreach (string filePath in files)
                {
                    string sql = await ReadSqlFromFileAsync(filePath, cancellationToken);
                    await dbConn.ExecuteAsync(sql, transaction: tx);
                }

                await RecordVersionAsync(dbConn, tx, version);
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }

    private static Task EnsureSchemaInfoTableAsync(IDbConnection db)
    {
        const string sql = """
                              CREATE TABLE IF NOT EXISTS [SchemaInfo]
                              (
                                  [Version] INTEGER PRIMARY KEY,
                                  [AppliedUtc] TEXT NOT NULL
                              );
                           """;
        Task<int> conditionalTableCreationTask = db.ExecuteAsync(sql);
        return conditionalTableCreationTask;
    }

    private static Task<int> GetCurrentSchemaVersionAsync(IDbConnection db)
    {
        const string sql = "SELECT COALESCE(MAX([Version]), 0) FROM [SchemaInfo];";
        Task<int> resultTask = db.ExecuteScalarAsync<int>(sql);
        return resultTask;
    }

    private static IReadOnlyList<string> GetOrderedMigrationFiles(int version)
    {
        IReadOnlyList<string> result = [];
        string versionString = $"v{version:000}";
        string folder = Path.Combine(AppContext.BaseDirectory, MigrationsRootFolder, versionString);

        if (Directory.Exists(folder))
            result = Directory.GetFiles(folder, "*.sql", SearchOption.TopDirectoryOnly)
                              .OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
                              .ToList();

        return result;
    }

    private static Task<string> ReadSqlFromFileAsync(string filePath, CancellationToken cancellationToken)
        => File.ReadAllTextAsync(filePath, cancellationToken);

    private static Task RecordVersionAsync(IDbConnection db, IDbTransaction tx, int version)
    {
        const string sql = """
                              INSERT INTO [SchemaInfo] ([Version], [AppliedUtc])
                              VALUES (@Version, @AppliedUtc);
                           """;

        var writeParameters = new
                              {
                                  Version = version,
                                  AppliedUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)
                              };
        Task<int> writeTask = db.ExecuteAsync(sql, writeParameters, tx);
        return writeTask;
    }
}
