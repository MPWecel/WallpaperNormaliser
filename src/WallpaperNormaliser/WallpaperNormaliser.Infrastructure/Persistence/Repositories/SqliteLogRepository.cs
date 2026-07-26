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
        using IDbConnection dbConn = _connectionFactory.Create();

        const string sql = """
                              SELECT [Id],
                                     [CreatedUtc] AS [CreationDateUtc],
                                     [Severity],
                                     [Category],
                                     [Message],
                                     [CorrelationId],
                                     [SourceHash],
                                     [Exception] AS [ExceptionMessage]
                              FROM [Logs]
                              WHERE
                              (
                                  (@DateRangeFromUtc IS NULL OR [CreatedUtc] >= @DateRangeFromUtc) AND
                                  (@DateRangeToUtc IS NULL OR [CreatedUtc] <= @DateRangeToUtc) AND
                                  (@MinimumSeverity IS NULL OR [Severity] >= @MinimumSeverity) AND
                                  (@CorrelationId IS NULL OR [CorrelationId] = @CorrelationId) AND
                                  (@SourceHash IS NULL OR [SourceHash] = @SourceHash)
                              )
                              ORDER BY [CreatedUtc] DESC
                              LIMIT @Limit
                              OFFSET @Skip
                           """;

        IEnumerable<LogEntry> rows = await dbConn.QueryAsync<LogEntry>(
                                                                          sql,
                                                                          new
                                                                          {
                                                                              query.DateRangeFromUtc,
                                                                              query.DateRangeToUtc,
                                                                              query.MinimumSeverity,
                                                                              query.CorrelationId,
                                                                              query.SourceHash,
                                                                              query.Limit,
                                                                              query.Skip
                                                                          }
                                                                      );
        List<LogEntry> result = rows.ToList();

        return result;
    }

    public async Task WriteAsync(LogEntry entry, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConn = _connectionFactory.Create();

        const string sql = """
                              INSERT INTO [Logs] ([Id], [CreatedUtc], [Severity], [Category], [Message], [SourceHash], [CorrelationId], [Exception])
                              VALUES (@Id, @CreatedUtc, @Severity, @Category, @Message, @SourceHash, @CorrelationId, @Exception)
                           """;

        await dbConn.ExecuteAsync(sql, ToParameters(entry));
    }

    public async Task WriteManyAsync(IEnumerable<LogEntry> entries, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConn = _connectionFactory.Create();
        using IDbTransaction tx = dbConn.BeginTransaction();

        foreach (LogEntry item in entries)
            await WriteInternalAsync(dbConn, tx, item, cancellationToken);

        tx.Commit();
    }

    public async Task<int> CleanupAsync(LogRetentionPolicy policy, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConn = _connectionFactory.Create();

        const string selectScript = "SELECT COUNT(*) FROM [Logs]";
        int initialCount = await dbConn.ExecuteScalarAsync<int>(selectScript);

        const string timeBasedDelete = """
                                          DELETE FROM [Logs]
                                          WHERE [CreatedUtc] < datetime('now', '-90 days')
                                       """;
        await dbConn.ExecuteAsync(timeBasedDelete);

        int countAfterTimePurge = await dbConn.ExecuteScalarAsync<int>(selectScript);

        if (countAfterTimePurge > 10000)
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
            await dbConn.ExecuteAsync(countBasedDelete);
        }

        int finalCount = await dbConn.ExecuteScalarAsync<int>(selectScript);
        int deletedCount = initialCount - finalCount;
        
        return deletedCount;
    }

    public async Task<long> CountAsync(LogQuery query, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConn = _connectionFactory.Create();

        const string sql = """
                              SELECT COUNT(*)
                              FROM [Logs]
                              WHERE
                              (
                                  (@DateRangeFromUtc IS NULL OR [CreatedUtc] >= @DateRangeFromUtc) AND
                                  (@DateRangeToUtc IS NULL OR [CreatedUtc] <= @DateRangeToUtc) AND
                                  (@MinimumSeverity IS NULL OR [Severity] >= @MinimumSeverity) AND
                                  (@CorrelationId IS NULL OR [CorrelationId] = @CorrelationId) AND
                                  (@SourceHash IS NULL OR [SourceHash] = @SourceHash)
                              )
                           """;

        long count = await dbConn.ExecuteScalarAsync<long>(
                                                              sql,
                                                              new
                                                              {
                                                                  query.DateRangeFromUtc,
                                                                  query.DateRangeToUtc,
                                                                  query.MinimumSeverity,
                                                                  query.CorrelationId,
                                                                  query.SourceHash
                                                              }
                                                          );
        return count;
    }

    private static Task WriteInternalAsync(
                                              IDbConnection dbConnection, 
                                              IDbTransaction transaction, 
                                              LogEntry entry, 
                                              CancellationToken cancellationToken = default
                                          )
    {
        const string sql = """
                              INSERT INTO [Logs] ([Id], [CreatedUtc], [Severity], [Category], [Message], [CorrelationId], [SourceHash], [Exception])
                              VALUES (@Id, @CreatedUtc, @Severity, @Category, @Message, @CorrelationId, @SourceHash, @Exception)
                           """;

        object parameters = ToParameters(entry);
        Task<int> internalWriteTask = dbConnection.ExecuteAsync(sql, parameters, transaction);
        
        return internalWriteTask;
    }

    private static object ToParameters(LogEntry entry) => new
                                                          {
                                                              Id = entry.Id,
                                                              CreatedUtc = entry.CreationDateUtc,
                                                              Severity = entry.Severity,
                                                              Category = entry.Category,
                                                              Message = entry.Message,
                                                              SourceHash = entry.SourceHash,
                                                              CorrelationId = entry.CorrelationId,
                                                              Exception = entry.ExceptionMessage
                                                          };
}
