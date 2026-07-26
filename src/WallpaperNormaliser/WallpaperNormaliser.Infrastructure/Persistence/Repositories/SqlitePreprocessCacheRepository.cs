using System.Data;

using Dapper;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Cache;
using WallpaperNormaliser.Infrastructure.Persistence.Database;

namespace WallpaperNormaliser.Infrastructure.Persistence.Repositories;

public sealed class SqlitePreprocessCacheRepository(SqliteConnectionFactory connectionFactory) : IPreprocessCacheRepository
{
    private readonly SqliteConnectionFactory _connectionFactory = connectionFactory;

    public async Task<PreprocessCacheEntry?> GetAsync(string sourceHash, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConn = _connectionFactory.Create();
        const string selectScript = """
                                       SELECT [SourceHash], [Resolution], [JpegQuality], [OutputBytes], [CreatedUtc], [ExpiresUtc] 
                                       FROM [PreprocessCache] 
                                       WHERE [SourceHash] = @sourceHash
                                    """;
        PreprocessCacheEntry? result = await dbConn.QueryFirstOrDefaultAsync<PreprocessCacheEntry>(selectScript, new { sourceHash });
        return result;
    }

    public async Task UpsertAsync(PreprocessCacheEntry entry, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConn = _connectionFactory.Create();
        const string upsertScript = """
                                       INSERT INTO [PreprocessCache]
                                           ([SourceHash], [Width], [Height], [Quality], [Bytes], [CreatedUtc], [ExpiresUtc])
                                       VALUES
                                           (@SourceHash, @Width, @Height, @Quality, @Bytes, @CreatedUtc, @ExpiresUtc)
                                       ON CONFLICT([SourceHash],[Width],[Height],[Quality])
                                           DO UPDATE SET [Bytes]=excluded.[Bytes],
                                                         [CreatedUtc]=excluded.[CreatedUtc],
                                                         [ExpiresUtc]=excluded.[ExpiresUtc]
                                    """;
        await dbConn.ExecuteAsync(
                                     upsertScript, 
                                     new 
                                     { 
                                         SourceHash = entry.SourceHash, 
                                         Width = entry.Resolution.Width, 
                                         Height = entry.Resolution.Height, 
                                         Quality = entry.Quality, 
                                         Bytes = entry.OutputBytes, 
                                         CreatedUtc = entry.CreatedUtc, 
                                         ExpiresUtc = entry.ExpiresUtc 
                                     }
                                 );
    }

    public async Task RemoveByHashAsync(string sourceHash, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConn = _connectionFactory.Create();
        const string deleteScript = "DELETE FROM [PreprocessCache] WHERE [SourceHash]=@sourceHash";
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
