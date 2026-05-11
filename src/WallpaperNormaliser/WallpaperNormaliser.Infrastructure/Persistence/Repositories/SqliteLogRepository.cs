using System.Data;

using Dapper;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Logging;
using WallpaperNormaliser.Infrastructure.Persistence.Database;

namespace WallpaperNormaliser.Infrastructure.Persistence.Repositories;
public sealed class SqliteLogRepository(SqliteConnectionFactory connectionFactory) : ILogRepository
{
    private readonly SqliteConnectionFactory _connectionFactory = connectionFactory;

    public async Task<IReadOnlyList<LogEntry>> QueryAsync(LogQuery query, CancellationToken cancellationToken = default)
    {
        using IDbConnection db = _connectionFactory.Create();

        const string sql = """
                              SELECT [Id], [CreationDateUtc], [Severity], [Category], [Message], [CorrelationId], [SourceHash], [ExceptionMessage] 
                              FROM [Logs] 
                              WHERE 
                              (
                                (@DateRangeFromUtc IS NULL OR [CreationDateUtc] >= @DateRangeFromUtc) AND
                                (@DateRangeToUtc IS NULL OR [CreationDateUtc] <= @DateRangeToUtc) AND
                                (@MinimumSeverity IS NULL OR [Severity] >= @MinimumSeverity) AND
                                (@CorrelationId IS NULL OR [CorrelationId] = @CorrelationId)
                                (@SourceHash IS NULL OR [SourceHash] = @SourceHash)
                              )
                              ORDER BY [CreatedUtc] DESC 
                              LIMIT @Limit
                              OFFSET @Skip
                           """;

        IEnumerable<LogEntry> rows = await db.QueryAsync<LogEntry>(
                                                                    sql, 
                                                                    new { query.DateRangeFromUtc, query.DateRangeToUtc, query.MinimumSeverity, query.CorrelationId, query.SourceHash, query.Limit }
                                                                  );
        return rows.ToList();
    }

    public async Task WriteAsync(LogEntry entry, CancellationToken cancellationToken = default)
    {
        using IDbConnection db = _connectionFactory.Create();

        const string sql = """
                              INSERT INTO [Logs] ([Id], [CreatedUtc], [Severity], [Category], [Message], [SourceHash], [CorrelationId], [Exception]) 
                              VALUES (@Id, @CreatedUtc, @Severity, @Category, @Message, @SourceHash, @CorrelationId, @Exception)
                           """;
        await db.ExecuteAsync(sql, entry);
    }

    public async Task WriteManyAsync(IEnumerable<LogEntry> entries, CancellationToken cancellationToken = default)
    {
        using IDbConnection db = _connectionFactory.Create();
        using IDbTransaction tx = db.BeginTransaction();

        foreach(var item in entries)
            await WriteInternalAsync(db, tx, item, cancellationToken);

        tx.Commit();
    }

    private static Task WriteInternalAsync(IDbConnection db, IDbTransaction transaction, LogEntry entry, CancellationToken cancellationToken = default)
    {
        string insertScript = """
                                 INSERT INTO [Logs] ([Id],[CreatedUtc],[Severity],[Category],[Message],[CorrelationId],[SourceHash],[Exception]) 
                                 VALUES (@Id, @CreatedUtc, @Severity, @Category, @Message, @CorrelationId, @SourceHash, @Exception)
                              """;
        return db.ExecuteAsync(insertScript, entry, transaction);
    }

    public async Task<int> CleanupAsync(LogRetentionPolicy policy, CancellationToken cancellationToken = default)
    {
        using IDbConnection db = _connectionFactory.Create();
        //using var tx = db.BeginTransaction();

        const string selectScript = "SELECT COUNT(*) FROM [Logs]";
        int initialLogEntryCount = await db.ExecuteScalarAsync<int>(selectScript);

        const string timeBasedDelete = """
                                          DELETE FROM [Logs] 
                                          WHERE [CreatedUtc] < datetime('now', '-90 days')
                                       """;
        await db.ExecuteAsync(timeBasedDelete);

        int logEntryCountAfterTimeBasedDelete = await db.ExecuteScalarAsync<int>(selectScript);

        if(logEntryCountAfterTimeBasedDelete > 10000)
        {
            const string countBasedDelete = """
                                               DELETE FROM [Logs]
                                               WHERE [Id] NOT IN
                                               (
                                                  SELECT [Id] 
                                                  FROM [Logs] 
                                                  ORDER BY [CreatedUtc] DESC 
                                                  LIMIT 2500
                                               )
                                            """;
            await db.ExecuteAsync(countBasedDelete);
        }

        int logEntryCountAfterCountBasedDelete = await db.ExecuteScalarAsync<int>(selectScript);

        int entriesRemovedCount = initialLogEntryCount - Math.Min(logEntryCountAfterTimeBasedDelete, 
                                                                  logEntryCountAfterCountBasedDelete);
        //tx.Commit();

        return entriesRemovedCount;
    }

    public async Task<long> CountAsync(LogQuery query, CancellationToken cancellationToken = default)
    {
        using IDbConnection db = _connectionFactory.Create();
        const string queryString = """
                                      SELECT COUNT(*)
                                      FROM [Logs]
                                      WHERE 
                                      (
                                          (@DateRangeFromUtc IS NULL OR [CreationDateUtc] >= @DateRangeFromUtc) AND
                                          (@DateRangeToUtc IS NULL OR [CreationDateUtc] <= @DateRangeToUtc) AND
                                          (@MinimumSeverity IS NULL OR [Severity] >= @MinimumSeverity) AND
                                          (@CorrelationId IS NULL OR [CorrelationId] = @CorrelationId)
                                          (@SourceHash IS NULL OR [SourceHash] = @SourceHash)      
                                      )
                                   """;
        long result = await db.ExecuteScalarAsync<long>(
                                                           queryString, 
                                                           query
                                                       );
        return result;
    }
}
