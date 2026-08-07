using System.Data;

using Dapper;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Cache;
using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Infrastructure.Persistence.Database;

namespace WallpaperNormaliser.Infrastructure.Persistence.Repositories;

public sealed class SqlitePreprocessCacheRepository(SqliteConnectionFactory connectionFactory) : IPreprocessCacheRepository
{
    private readonly SqliteConnectionFactory _connectionFactory = connectionFactory;

    private const string SelectSql = """
                                        SELECT [SourceHash], [Resolution], [JpegQuality], [OutputBytes], 
                                               [CreatedUtc], [ExpiresUtc]
                                        FROM [PreprocessCache]
                                     """;

    private const string UpsertSql = """
                                        INSERT INTO [PreprocessCache]
                                        (
                                            [SourceHash], [Resolution], [JpegQuality], [OutputBytes], 
                                            [CreatedUtc], [ExpiresUtc]
                                        )
                                        VALUES
                                        (
                                            @SourceHash, @Resolution, @JpegQuality, @OutputBytes, 
                                            @CreatedUtc, @ExpiresUtc
                                        )
                                        ON CONFLICT([SourceHash], [Resolution], [JpegQuality]) DO UPDATE SET [OutputBytes] = excluded.[OutputBytes],
                                                                                                             [CreatedUtc] = excluded.[CreatedUtc],
                                                                                                             [ExpiresUtc] = excluded.[ExpiresUtc]
                                     """;

    public async Task<PreprocessCacheEntry?> GetAsync(
                                                         string sourceHash, 
                                                         Resolution resolution, 
                                                         int quality, 
                                                         CancellationToken cancellationToken = default
                                                     )
    {
        using IDbConnection dbConn = _connectionFactory.Create();
        const string selectScript = $"""
                                       {SelectSql}
                                       WHERE [SourceHash] = @SourceHash
                                         AND [Resolution] = @Resolution
                                         AND [JpegQuality] = @JpegQuality
                                    """;
        PreprocessCacheEntry? result = 
            await dbConn.QueryFirstOrDefaultAsync<PreprocessCacheEntry>(
                                                                           selectScript,
                                                                           new
                                                                           {
                                                                               SourceHash  = sourceHash,
                                                                               Resolution  = resolution,
                                                                               JpegQuality = quality
                                                                           }
                                                                       );
        return result;
    }

    public async Task UpsertAsync(PreprocessCacheEntry entry, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConn = _connectionFactory.Create();
        await dbConn.ExecuteAsync(
                                     UpsertSql,
                                     new
                                     {
                                         SourceHash = entry.SourceHash,
                                         Resolution = entry.Resolution,
                                         JpegQuality = entry.Quality,
                                         OutputBytes = entry.OutputBytes,
                                         CreatedUtc = entry.CreatedUtc,
                                         ExpiresUtc = entry.ExpiresUtc
                                     }
                                 );
    }

    public async Task RemoveByHashAsync(string sourceHash, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConn = _connectionFactory.Create();
        const string deleteScript = "DELETE FROM [PreprocessCache] WHERE [SourceHash] = @sourceHash";
        await dbConn.ExecuteAsync(deleteScript, new { sourceHash });
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConn = _connectionFactory.Create();
        const string deleteScript = "DELETE FROM [PreprocessCache]";
        await dbConn.ExecuteAsync(deleteScript);
    }

    public async Task CleanupExpiredAsync(CancellationToken ct = default)
    {
        using IDbConnection dbConn = _connectionFactory.Create();
        DateTime utcNow = DateTime.UtcNow;
        const string deleteScript = "DELETE FROM [PreprocessCache] WHERE [ExpiresUtc] < @utcNow";
        await dbConn.ExecuteAsync(deleteScript, new { utcNow });
    }
}
