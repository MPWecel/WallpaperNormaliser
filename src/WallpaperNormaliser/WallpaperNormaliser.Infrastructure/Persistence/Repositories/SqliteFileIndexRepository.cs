using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Indexing;
using WallpaperNormaliser.Infrastructure.Persistence.Database;

namespace WallpaperNormaliser.Infrastructure.Persistence.Repositories;
public sealed class SqliteFileIndexRepository : IFileIndexRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteFileIndexRepository(SqliteConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<FileIndexEntry?> GetByHashAsync(string hash, CancellationToken cancellationToken = default)
    {
        using IDbConnection db = _connectionFactory.Create();
        const string queryString = """
                                      SELECT [Id], [Hash], [RelativePath], [FullPath], [Resolution], [SizeBytes], [LastSeenUtc]
                                      FROM [FileIndex]
                                      WHERE [Hash] = @hash
                                   """;
        FileIndexEntry? result = await db.QuerySingleOrDefaultAsync<FileIndexEntry>(queryString, new { hash });
        return result;
    }

    public async Task<FileIndexEntry?> GetByRelativePathAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        using IDbConnection db = _connectionFactory.Create();
        const string queryString = """
                                      SELECT [Id], [Hash], [RelativePath], [FullPath], [Resolution], [SizeBytes], [LastSeenUtc]
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
                                SELECT [Id], [Hash], [RelativePath], [FullPath], [Resolution], [SizeBytes], [LastSeenUtc]
                                FROM [FileIndex]
                                WHERE [Hash]=@hash
                             """;
        IEnumerable<FileIndexEntry> rows = await db.QueryAsync<FileIndexEntry>(query, new { hash });
        List<FileIndexEntry> result = rows.ToList();
        return result;
    }

    public async Task<IReadOnlyList<FileIndexEntry>> ListAsync(CancellationToken ct = default)
    {
        using IDbConnection db = _connectionFactory.Create();
        const string query = """
                                SELECT [Id], [Hash], [RelativePath], [FullPath], [Resolution], [SizeBytes], [LastSeenUtc]
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
                                INSERT INTO [FileIndex] ([Id], [Hash], [RelativePath], [FullPath], [Width], [Height], [Bytes], [LastSeenUtc])
                                VALUES (@Id, @Hash, @RelativePath, @FullPath, @Width, @Height, @Bytes, @LastSeenUtc)
                                ON CONFLICT([Hash]) DO UPDATE SET [RelativePath]=excluded.[RelativePath],
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
                        INSERT INTO [FileIndex] ([Id], [Hash], [RelativePath], [FullPath], [Width], [Height], [Bytes], [LastSeenUtc])
                        VALUES (@Id, @Hash, @RelativePath, @FullPath, @Width, @Height, @Bytes, @LastSeenUtc)
                        ON CONFLICT([Hash]) DO UPDATE SET [RelativePath]=excluded.[RelativePath],
                                                          [FullPath]=excluded.[FullPath],
                                                          [Width]=excluded.[Width],
                                                          [Height]=excluded.[Height],
                                                          [Bytes]=excluded.[Bytes],
                                                          [LastSeenUtc]=excluded.[LastSeenUtc]
                     """;
        foreach(var entry in entries)
        {
            await db.ExecuteAsync(query, entry, transaction);
        }
        transaction.Commit();
    }

    public async Task RemoveMissingAsync(IReadOnlyCollection<string> existingRelativePaths, CancellationToken ct = default)
    {
        using IDbConnection db = _connectionFactory.Create();
        using IDbTransaction transaction = db.BeginTransaction();
        const string selecthPathsQuery = "SELECT [RelativePath] FROM [FileIndex]";
        IEnumerable<string> all = await db.QueryAsync<string>(selecthPathsQuery);
        IEnumerable<string> remove = all.Except(existingRelativePaths);
        const string deleteCommand = "DELETE FROM [FileIndex] WHERE [RelativePath]=@path";

        foreach (var path in remove) 
            await db.ExecuteAsync(deleteCommand, new { path }, transaction);

        transaction.Commit();
    }
}
