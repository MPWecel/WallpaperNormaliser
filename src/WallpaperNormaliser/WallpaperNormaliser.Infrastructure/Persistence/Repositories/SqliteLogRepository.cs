using System;
using System.Collections.Generic;
using System.Data;
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

        string sql = """
                        SELECT [Id], [CreationDateUtc], [Severity], [Category], [Message], [CorrelationId], [SourceHash], [ExceptionMessage] 
                        FROM [Logs] 
                        ORDER BY [CreatedUtc] 
                        DESC LIMIT @Limit
                     """;

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

    public async Task WriteManyAsync(IEnumerable<LogEntry> entries, CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();
        using var tx = db.BeginTransaction();

        foreach(var item in entries)
        {
            await WriteInternalAsync(db, tx, item);
        }

        tx.Commit();
    }

    private Task WriteInternalAsync(IDbConnection db, IDbTransaction transaction, LogEntry entry, CancellationToken cancellationToken = default)
    {
        string insertScript = """
                                 INSERT INTO [Logs] ([Id],[CreatedUtc],[Severity],[Category],[Message],[CorrelationId],[SourceHash],[Exception]) 
                                 VALUES (@Id, @CreatedUtc, @Severity, @Category, @Message, @CorrelationId, @SourceHash, @Exception)
                              """;
        return db.ExecuteAsync(insertScript, entry, transaction);
    }

    public async Task<int> CleanupAsync(LogRetentionPolicy policy, CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();
        //using var tx = db.BeginTransaction();

        string selectScript = "SELECT COUNT(*) FROM [Logs]";
        int initialLogEntryCount = await db.ExecuteScalarAsync<int>(selectScript);

        string timeBasedDelete = "DELETE FROM [Logs] WHERE [CreatedUtc] < datetime('now', '-90 days')";
        await db.ExecuteAsync(timeBasedDelete);

        int logEntryCountAfterTimeBasedDelete = await db.ExecuteScalarAsync<int>(selectScript);

        if(logEntryCountAfterTimeBasedDelete > 10000)
        {
            string countBasedDelete = """
                                         DELETE FROM [Logs]
                                         WHERE [Id] NOT IN
                                         (
                                            SELECT [Id] FROM [Logs] ORDER BY [CreatedUtc] DESC LIMIT 2500
                                         )
                                      """;
            await db.ExecuteAsync(countBasedDelete);
        }

        int logEntryCountAfterCountBasedDelete = await db.ExecuteScalarAsync<int>(selectScript);

        int entriesRemovedCount = initialLogEntryCount - Math.Min(logEntryCountAfterTimeBasedDelete, logEntryCountAfterCountBasedDelete);

        //tx.Commit();

        return entriesRemovedCount;
    }

    public async Task<long> CountAsync(LogQuery query, CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();

        string queryString = """
                                SELECT COUNT(*)
                                FROM [Logs]
                                WHERE (@MinimumSeverity IS NULL OR [Severity]=@MinimumSeverity) AND (@CorrelationId IS NULL OR [CorrelationId]=@CorrelationId)
                             """;
        long result = await db.ExecuteScalarAsync<long>(queryString, query);
        return result;
    }
}
