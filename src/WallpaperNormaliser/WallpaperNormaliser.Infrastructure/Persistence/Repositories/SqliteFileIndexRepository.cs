using System.Data;

using Dapper;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Enums;
using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Core.Models.Indexing;
using WallpaperNormaliser.Infrastructure.Persistence.Database;

namespace WallpaperNormaliser.Infrastructure.Persistence.Repositories;
public sealed class SqliteFileIndexRepository(SqliteConnectionFactory connectionFactory) : IFileIndexRepository
{
    private readonly SqliteConnectionFactory _connectionFactory = connectionFactory;

    private const string UpsertSql = """
                                        INSERT INTO [FileIndex] ([Id], [SourceHash], [FileName], [RelativePath], [FullPath], [Format], [SizeBytes], [Width], [Height], [LastSeenUtc], [LastWriteUtc], [IsDuplicate])
                                        VALUES (@Id, @SourceHash, @FileName, @RelativePath, @FullPath, @Format, @SizeBytes, @Width, @Height, @LastSeenUtc, @LastWriteUtc, @IsDuplicate)
                                        ON CONFLICT([SourceHash]) DO UPDATE SET [FileName]=excluded.[FileName],
                                                                                [RelativePath]=excluded.[RelativePath],
                                                                                [FullPath]=excluded.[FullPath],
                                                                                [Format]=excluded.[Format],
                                                                                [SizeBytes]=excluded.[SizeBytes],
                                                                                [Width]=excluded.[Width],
                                                                                [Height]=excluded.[Height],
                                                                                [LastSeenUtc]=excluded.[LastSeenUtc],
                                                                                [LastWriteUtc]=excluded.[LastWriteUtc],
                                                                                [IsDuplicate]=excluded.[IsDuplicate]
                                     """;

    public async Task<FileIndexEntry?> GetByHashAsync(string hash, CancellationToken cancellationToken = default)
    {
        using IDbConnection db = _connectionFactory.Create();
        const string queryString = """
                                      SELECT [Id], [SourceHash], 
                                             [FileName], [RelativePath], [FullPath], 
                                             [Format], [SizeBytes], [Width], [Height], 
                                             [LastSeenUtc], [LastWriteUtc], [IsDuplicate]
                                      FROM [FileIndex]
                                      WHERE [SourceHash] = @hash
                                   """;
        FileIndexEntry? result = (await db.QuerySingleOrDefaultAsync<FileIndexRow>(queryString, new { hash }))
                                    ?.FromRow() ?? null;
        return result;
    }

    public async Task<FileIndexEntry?> GetByRelativePathAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        using IDbConnection db = _connectionFactory.Create();
        const string queryString = """
                                      SELECT [Id], [SourceHash], 
                                             [FileName], [RelativePath], [FullPath], 
                                             [Format], [SizeBytes], [Width], [Height], 
                                             [LastSeenUtc], [LastWriteUtc], [IsDuplicate]
                                      FROM [FileIndex]
                                      WHERE [RelativePath] = @relativePath
                                   """;
        FileIndexEntry? result = (await db.QuerySingleOrDefaultAsync<FileIndexRow>(queryString, new { relativePath }))
                                    ?.FromRow() ?? null;
        return result;
    }

    public async Task<IReadOnlyList<FileIndexEntry>> GetDuplicatesAsync(string hash, CancellationToken ct = default)
    {
        using IDbConnection db = _connectionFactory.Create();
        const string query = """
                                SELECT [Id], [SourceHash], 
                                       [FileName], [RelativePath], [FullPath], 
                                       [Format], [SizeBytes], [Width], [Height], 
                                       [LastSeenUtc], [LastWriteUtc], [IsDuplicate]
                                FROM [FileIndex]
                                WHERE [SourceHash]=@hash
                             """;
        IEnumerable<FileIndexEntry> rows = (await db.QueryAsync<FileIndexRow>(query, new { hash }))
                                            .Select(x=>x.FromRow());
        List<FileIndexEntry> result = rows.ToList();
        return result;
    }

    public async Task<IReadOnlyList<FileIndexEntry>> ListAsync(CancellationToken ct = default)
    {
        using IDbConnection db = _connectionFactory.Create();
        const string query = """
                                SELECT [Id], [SourceHash], 
                                       [FileName], [RelativePath], [FullPath], 
                                       [Format], [SizeBytes], [Width], [Height], 
                                       [LastSeenUtc], [LastWriteUtc], [IsDuplicate]
                                FROM [FileIndex]
                                ORDER BY [RelativePath]
                             """;
        IEnumerable<FileIndexEntry> rows = (await db.QueryAsync<FileIndexRow>(query)).Select(x=>x.FromRow());
        List<FileIndexEntry> result = rows.ToList();
        return result;
    }

    public async Task UpsertAsync(FileIndexEntry entry, CancellationToken ct = default)
    {
        using IDbConnection db = _connectionFactory.Create();
        await db.ExecuteAsync(UpsertSql, FileIndexRow.ToRow(entry));
    }

    public async Task UpsertManyAsync(IReadOnlyCollection<FileIndexEntry> entries, CancellationToken ct = default)
    {
        using IDbConnection db = _connectionFactory.Create();
        using IDbTransaction transaction = db.BeginTransaction();

        try
        {
            foreach(var entry in entries)
            {
                await db.ExecuteAsync(UpsertSql, FileIndexRow.ToRow(entry), transaction);
            }
            transaction.Commit();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            transaction.Rollback();
            throw new InvalidOperationException(
                $"[SqliteFileIndexRepository.UpsertManyAsync] Transaction failed. SQL: {UpsertSql}",
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
    internal sealed record FileIndexRow(
                                           string Id,
                                           string SourceHash,
                                           string FileName,
                                           string RelativePath,
                                           string? FullPath,
                                           int Format,
                                           long SizeBytes,
                                           long? Width,
                                           long? Height,
                                           DateTime LastSeenUtc,
                                           DateTime? LastWriteUtc,
                                           bool IsDuplicate
                                       )
    {
        public FileIndexEntry FromRow() => new(
                                                  Id, 
                                                  SourceHash, 
                                                  FileName, 
                                                  RelativePath, 
                                                  FullPath, 
                                                  (EFileFormat)(Format), 
                                                  new Resolution((uint)(Width ?? 0), (uint)(Height ?? 0)), 
                                                  SizeBytes, 
                                                  LastSeenUtc, 
                                                  LastWriteUtc, 
                                                  IsDuplicate
                                              );

        public static FileIndexRow ToRow(FileIndexEntry entry) => new(
                                                                         entry.Id,
                                                                         entry.SourceHash,
                                                                         entry.FileName,
                                                                         entry.RelativePath,
                                                                         entry.FullPath,
                                                                         (int)(entry.Format),
                                                                         entry.SizeBytes,
                                                                         (long?)(entry.Resolution.Width),
                                                                         (long?)(entry.Resolution.Height),
                                                                         entry.LastSeenUtc,
                                                                         entry.LastWriteUtc,
                                                                         entry.IsDuplicate
                                                                     );
    }
}

