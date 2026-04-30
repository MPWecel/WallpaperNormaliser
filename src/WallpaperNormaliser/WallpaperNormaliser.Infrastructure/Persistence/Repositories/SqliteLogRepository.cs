using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Logging;
using WallpaperNormaliser.Infrastructure.Persistence.Database;

namespace WallpaperNormaliser.Infrastructure.Persistence.Repositories;
public sealed class SqliteLogRepository : ILogRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteLogRepository(SqliteConnectionFactory connectionFactory) 
        => _connectionFactory = connectionFactory;

    public async Task<IReadOnlyList<LogEntry>> QueryAsync(LogQuery query, CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();

        string sql = "SELECT * FROM [Logs] ORDER BY [CreatedUtc] DESC LIMIT @Limit";

        var rows = await db.QueryAsync<LogEntry>(sql, new { query.Limit });
        return rows.ToList();
    }

    public async Task WriteAsync(LogEntry entry, CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();

        string sql = """
                        INSERT INTO [Logs] ([Id], [CreatedUtc], [Severity], [Category], [Message], [SourceHash], [CorrelationId], [Exception]) 
                        VALUES (@Id, @CreatedUtc, @Severity, @Category, @Message, @SourceHash, @CorrelationId, @Exception)
                     """;

        await db.ExecuteAsync(sql, entry);
    }

    public Task WriteManyAsync(IEnumerable<LogEntry> entries, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task<int> CleanupAsync(LogRetentionPolicy policy, CancellationToken cancellationToken = default) => throw new NotImplementedException();

}
