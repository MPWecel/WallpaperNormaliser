using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallpaperNormaliser.Core.Models.Cache;
using WallpaperNormaliser.Infrastructure.Persistence.Database;

namespace WallpaperNormaliser.Infrastructure.Persistence.Repositories;
public sealed class SqlitePreprocessCacheRepository
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

}
