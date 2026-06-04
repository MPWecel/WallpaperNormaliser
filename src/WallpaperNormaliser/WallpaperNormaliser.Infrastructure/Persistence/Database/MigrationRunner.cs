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
        using IDbConnection db = _connectionFactory.Create();

        await EnsureSchemaInfoTableAsync(db);

        int currentVersion = await GetCurrentSchemaVersionAsync(db);
        if (currentVersion >= LatestSchemaVersion)
            return;

        for (int version = currentVersion + 1; version <= LatestSchemaVersion; version++)
        {
            IReadOnlyList<string> files = GetOrderedMigrationFiles(version);
            if (files.Count == 0)
                continue;

            using IDbTransaction tx = db.BeginTransaction();
            try
            {
                foreach (string filePath in files)
                {
                    string sql = await ReadSqlFromFileAsync(filePath, cancellationToken);
                    await db.ExecuteAsync(sql, transaction: tx);
                }

                await RecordVersionAsync(db, tx, version);
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
        return db.ExecuteAsync(sql);
    }

    private static async Task<int> GetCurrentSchemaVersionAsync(IDbConnection db)
    {
        const string sql = "SELECT COALESCE(MAX([Version]), 0) FROM [SchemaInfo];";
        return await db.ExecuteScalarAsync<int>(sql);
    }

    private static IReadOnlyList<string> GetOrderedMigrationFiles(int version)
    {
        string folder = Path.Combine(AppContext.BaseDirectory, MigrationsRootFolder, $"v{version:000}");
        if (!Directory.Exists(folder))
            return [];

        return Directory
            .GetFiles(folder, "*.sql", SearchOption.TopDirectoryOnly)
            .OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static Task<string> ReadSqlFromFileAsync(string filePath, CancellationToken cancellationToken)
        => File.ReadAllTextAsync(filePath, cancellationToken);

    private static Task RecordVersionAsync(IDbConnection db, IDbTransaction tx, int version)
    {
        const string sql = """
                              INSERT INTO [SchemaInfo] ([Version], [AppliedUtc])
                              VALUES (@Version, @AppliedUtc);
                           """;
        return db.ExecuteAsync(sql, new
        {
            Version    = version,
            AppliedUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)
        }, tx);
    }
}
