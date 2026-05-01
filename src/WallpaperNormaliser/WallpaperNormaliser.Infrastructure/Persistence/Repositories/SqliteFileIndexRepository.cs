using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallpaperNormaliser.Infrastructure.Persistence.Database;

namespace WallpaperNormaliser.Infrastructure.Persistence.Repositories;
public sealed class SqliteFileIndexRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteFileIndexRepository(SqliteConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<FileIndexEntry?> GetByHashAsync(
        string hash,
        CancellationToken ct = default)
    {
        using var db = _connectionFactory.Create();

        return await db.QuerySingleOrDefaultAsync<FileIndexEntry>(
            "SELECT * FROM [FileIndex] WHERE [Hash]=@hash",
            new { hash });
    }

    public async Task<FileIndexEntry?> GetByRelativePathAsync(
        string relativePath,
        CancellationToken ct = default)
    {
        using var db = _connectionFactory.Create();

        return await db.QuerySingleOrDefaultAsync<FileIndexEntry>(
            "SELECT * FROM [FileIndex] WHERE [RelativePath]=@relativePath",
            new { relativePath });
    }

    public async Task<IReadOnlyList<FileIndexEntry>> GetDuplicatesAsync(
        string hash,
        CancellationToken ct = default)
    {
        using var db = _connectionFactory.Create();

        var rows = await db.QueryAsync<FileIndexEntry>(
            "SELECT * FROM [FileIndex] WHERE [Hash]=@hash",
            new { hash });

        return rows.ToList();
    }

    public async Task<IReadOnlyList<FileIndexEntry>> ListAsync(
        CancellationToken ct = default)
    {
        using var db = _connectionFactory.Create();

        var rows = await db.QueryAsync<FileIndexEntry>(
            "SELECT * FROM [FileIndex] ORDER BY [RelativePath]");

        return rows.ToList();
    }

    public async Task UpsertAsync(
        FileIndexEntry entry,
        CancellationToken ct = default)
    {
        using var db = _connectionFactory.Create();

        await db.ExecuteAsync(
            """
            INSERT INTO [FileIndex]
            ([Id],[Hash],[RelativePath],[FullPath],[Width],[Height],
             [Bytes],[LastSeenUtc])
            VALUES
            (@Id,@Hash,@RelativePath,@FullPath,@Width,@Height,
             @Bytes,@LastSeenUtc)
            ON CONFLICT([Hash]) DO UPDATE SET
                [RelativePath]=excluded.[RelativePath],
                [FullPath]=excluded.[FullPath],
                [Width]=excluded.[Width],
                [Height]=excluded.[Height],
                [Bytes]=excluded.[Bytes],
                [LastSeenUtc]=excluded.[LastSeenUtc]
            """,
            entry);
    }

    public async Task UpsertManyAsync(
        IReadOnlyCollection<FileIndexEntry> entries,
        CancellationToken ct = default)
    {
        foreach (var e in entries)
            await UpsertAsync(e, ct);
    }

    public async Task RemoveMissingAsync(
        IReadOnlyCollection<string> existingRelativePaths,
        CancellationToken ct = default)
    {
        using var db = _connectionFactory.Create();

        var all = await db.QueryAsync<string>(
            "SELECT [RelativePath] FROM [FileIndex]");

        var remove = all.Except(existingRelativePaths).ToArray();

        foreach (var path in remove)
        {
            await db.ExecuteAsync(
                "DELETE FROM [FileIndex] WHERE [RelativePath]=@path",
                new { path });
        }
    }

}
