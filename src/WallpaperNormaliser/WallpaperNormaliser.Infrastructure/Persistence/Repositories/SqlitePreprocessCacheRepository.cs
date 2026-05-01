using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Cache;
using WallpaperNormaliser.Infrastructure.Persistence.Database;

namespace WallpaperNormaliser.Infrastructure.Persistence.Repositories;

public sealed class SqlitePreprocessCacheRepository : IPreprocessCacheRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqlitePreprocessCacheRepository(SqliteConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<PreprocessCacheEntry?> GetAsync(string sourceHash, CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();

        string selectScript = """
                                 SELECT [SourceHash], [Resolution], [JpegQuality], [OutputBytes], [CreatedUtc], [ExpiresUtc] 
                                 FROM [PreprocessCache] 
                                 WHERE [SourceHash] = @sourceHash
                              """;
        PreprocessCacheEntry? result = await db.QueryFirstOrDefaultAsync<PreprocessCacheEntry>(selectScript, new { sourceHash });
        return result;
    }

    public async Task UpsertAsync(PreprocessCacheEntry entry, CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();

        const string upsertScript = """
                                       INSERT INTO [PreprocessCache]
                                           ([SourceHash], [Width], [Height], [Quality], [Bytes], [CreatedUtc], [ExpiresUtc])
                                       VALUES
                                           (@SourceHash, @Width, @Height, @Quality, @Bytes, @CreatedUtc, @ExpiresUtc)
                                       ON CONFLICT([SourceHash],[Width],[Height],[Quality])
                                           DO UPDATE SET
                                               [Bytes]=excluded.[Bytes],
                                               [CreatedUtc]=excluded.[CreatedUtc],
                                               [ExpiresUtc]=excluded.[ExpiresUtc]
                                    """;
        await db.ExecuteAsync(upsertScript, entry);
    }

    public async Task RemoveByHashAsync(string sourceHash, CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();
        const string deleteScript = "DELETE FROM [PreprocessCache] WHERE [SourceHash]=@sourceHash";
        await db.ExecuteAsync(deleteScript, new { sourceHash });
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();
        const string deleteScript = "DELETE FROM [PreprocessCache]";
        await db.ExecuteAsync(deleteScript);
    }

    public async Task CleanupExpiredAsync(CancellationToken ct = default)
    {
        using var db = _connectionFactory.Create();
        DateTime utcNow = DateTime.UtcNow;
        const string deleteScript = "DELETE FROM [PreprocessCache] WHERE [ExpiresUtc] < @utcNow";
        await db.ExecuteAsync(deleteScript, new { utcNow });
    }

}
