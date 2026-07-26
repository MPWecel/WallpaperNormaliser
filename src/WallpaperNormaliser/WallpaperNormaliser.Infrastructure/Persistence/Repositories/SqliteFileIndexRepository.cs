using System.Data;

using Dapper;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Indexing;
using WallpaperNormaliser.Infrastructure.Persistence.Database;

namespace WallpaperNormaliser.Infrastructure.Persistence.Repositories;
public sealed class SqliteFileIndexRepository(SqliteConnectionFactory connectionFactory) : IFileIndexRepository
{
    private readonly SqliteConnectionFactory _connectionFactory = connectionFactory;

    public async Task<FileIndexEntry?> GetByHashAsync(string hash, CancellationToken cancellationToken = default)
    {
        using IDbConnection db = _connectionFactory.Create();
        const string queryString = """
                                      SELECT [Id], [SourceHash], [FileName], [RelativePath], [FullPath], [Format], [SizeBytes], [Width], [Height] [LastSeenUtc], [LastWriteUtc], [IsDuplicate]
                                      FROM [FileIndex]
                                      WHERE [SourceHash] = @SourceHash
                                   """;
        FileIndexEntry? result = await db.QuerySingleOrDefaultAsync<FileIndexEntry>(queryString, new { SourceHash = hash });
        return result;
    }

    public async Task<FileIndexEntry?> GetByRelativePathAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        using IDbConnection db = _connectionFactory.Create();
        const string queryString = """
                                      SELECT [Id], [SourceHash], [FileName], [RelativePath], [FullPath], [Format], [SizeBytes], [Width], [Height] [LastSeenUtc], [LastWriteUtc], [IsDuplicate]
                                      FROM [FileIndex]
                                      WHERE [RelativePath] = @relativePath
                                   """;
        FileIndexEntry? result = await db.QuerySingleOrDefaultAsync<FileIndexEntry>(queryString, new { relativePath });
        return result;
    }

    public async Task<IReadOnlyList<FileIndexEntry>> GetDuplicatesAsync(string hash, CancellationToken ct = default)
    {
        using IDbConnection db = _connectionFactory.Create();
        const string query = """
                                SELECT [Id], [SourceHash], [FileName], [RelativePath], [FullPath], [Format], [SizeBytes], [Width], [Height] [LastSeenUtc], [LastWriteUtc], [IsDuplicate]
                                FROM [FileIndex]
                                WHERE [SourceHash]=@SourceHash
                             """;
        IEnumerable<FileIndexEntry> rows = await db.QueryAsync<FileIndexEntry>(query, new { hash });
        List<FileIndexEntry> result = rows.ToList();
        return result;
    }

    public async Task<IReadOnlyList<FileIndexEntry>> ListAsync(CancellationToken ct = default)
    {
        using IDbConnection db = _connectionFactory.Create();
        const string query = """
                                SELECT [Id], [SourceHash], [FileName], [RelativePath], [FullPath], [Format], [SizeBytes], [Width], [Height] [LastSeenUtc], [LastWriteUtc], [IsDuplicate]
                                FROM [FileIndex]
                                ORDER BY [RelativePath]
                             """;
        IEnumerable<FileIndexEntry> rows = await db.QueryAsync<FileIndexEntry>(query);
        List<FileIndexEntry> result = rows.ToList();
        return result;
    }

    public async Task UpsertAsync(FileIndexEntry entry, CancellationToken ct = default)
    {
        using IDbConnection db = _connectionFactory.Create();
        const string query = """
                                INSERT INTO [FileIndex] ([Id], [SourceHash], [RelativePath], [FullPath], [Width], [Height], [Bytes], [LastSeenUtc])
                                VALUES (@Id, @SourceHash, @RelativePath, @FullPath, @Width, @Height, @Bytes, @LastSeenUtc)
                                ON CONFLICT([SourceHash]) DO UPDATE SET [RelativePath]=excluded.[RelativePath],
                                                                  [FullPath]=excluded.[FullPath],
                                                                  [Width]=excluded.[Width],
                                                                  [Height]=excluded.[Height],
                                                                  [Bytes]=excluded.[Bytes],
                                                                  [LastSeenUtc]=excluded.[LastSeenUtc]
                             """;
        await db.ExecuteAsync(query, entry);
    }

    public async Task UpsertManyAsync(IReadOnlyCollection<FileIndexEntry> entries, CancellationToken ct = default)
    {
        using IDbConnection db = _connectionFactory.Create();
        using IDbTransaction transaction = db.BeginTransaction();
        const string query = """
                        INSERT INTO [FileIndex] ([Id], [SourceHash], [RelativePath], [FullPath], [Width], [Height], [Bytes], [LastSeenUtc])
                        VALUES (@Id, @SourceHash, @RelativePath, @FullPath, @Width, @Height, @Bytes, @LastSeenUtc)
                        ON CONFLICT([SourceHash]) DO UPDATE SET [RelativePath]=excluded.[RelativePath],
                                                          [FullPath]=excluded.[FullPath],
                                                          [Width]=excluded.[Width],
                                                          [Height]=excluded.[Height],
                                                          [Bytes]=excluded.[Bytes],
                                                          [LastSeenUtc]=excluded.[LastSeenUtc]
                     """;
        try
        {
            foreach(var entry in entries)
            {
                await db.ExecuteAsync(query, entry, transaction);
            }
            transaction.Commit();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            transaction.Rollback();
            throw new InvalidOperationException(
                $"[SqliteFileIndexRepository.UpsertManyAsync] Transaction failed. SQL: {query}",
                ex);
        }
    }

    public async Task RemoveMissingAsync(IReadOnlyCollection<string> existingRelativePaths, CancellationToken ct = default)
    {
        using IDbConnection db = _connectionFactory.Create();
        using IDbTransaction transaction = db.BeginTransaction();
        const string selecthPathsQuery = "SELECT [RelativePath] FROM [FileIndex]";
        const string deleteCommand = "DELETE FROM [FileIndex] WHERE [RelativePath]=@path";

        try
        {
            IEnumerable<string> all = await db.QueryAsync<string>(selecthPathsQuery);
            IEnumerable<string> remove = all.Except(existingRelativePaths);

            foreach (var path in remove)
                await db.ExecuteAsync(deleteCommand, new { path }, transaction);

            transaction.Commit();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            transaction.Rollback();
            throw new InvalidOperationException(
                $"[SqliteFileIndexRepository.RemoveMissingAsync] Transaction failed. SQL: {deleteCommand}",
                ex);
        }
    }
}
