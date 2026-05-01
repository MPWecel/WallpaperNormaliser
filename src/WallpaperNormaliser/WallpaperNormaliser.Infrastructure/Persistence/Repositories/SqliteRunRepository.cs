using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models;
using WallpaperNormaliser.Core.Models.Processing;
using WallpaperNormaliser.Infrastructure.Persistence.Database;

namespace WallpaperNormaliser.Infrastructure.Persistence.Repositories;
public sealed class SqliteRunRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteRunRepository(SqliteConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task CreateRunAsync(ProcessingRun run, CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();
        string insertScript = """
                                 INSERT INTO [ProcessingRuns] ([RunId], [StartedUtc], [FinishedUtc], [Status], [TotalFiles], [SuccessCount], [FailedCount], [SkippedCount])
                                 VALUES (@RunId, @StartedUtc, @FinishedUtc, @TotalFiles, @SuccessCount, @FailedCount, @SkippedCount)
                              """;

        await db.ExecuteAsync(insertScript, run);
    }

    public async Task AddRunItemAsync(ProcessingRunItem item, CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();
        string insertScript = """
                                 INSERT INTO [ProcessingRunItems] ([Id], [RunId], [SourceHash], [FileName], [Status], [Message], [DurationMs], [CreatedUtc])
                                 VALUES (@Id, @RunId, @SourceHash, @FileName, @Status, @Message, @DurationMs, @CreatedUtc)
                              """;

        await db.ExecuteAsync(insertScript, item);
    }

    public async Task UpsertRunAsync(ProcessingRun run, CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();

        string selectScript = """
                                 SELECT [RunId], [StartedUtc], [FinishedUtc], [Status], [TotalFiles], [SuccessCount], [FailedCount], [SkippedCount]
                                 FROM [ProcessingRuns]
                                 WHERE [RunId] = @RunId
                              """;
        ProcessingRun? fromDb = await db.QuerySingleOrDefaultAsync<ProcessingRun>(selectScript, new { run.RunId });
        bool isRunInDb = (fromDb is not null);

        if (isRunInDb)
            await UpdateRunAsync(db, run, fromDb!, cancellationToken).ConfigureAwait(false);
        else
            await CreateRunAsync(run, cancellationToken).ConfigureAwait(false);
    }

    private async Task UpdateRunAsync(IDbConnection db, ProcessingRun run, ProcessingRun fromDb, CancellationToken cancellationToken)
    {
        List<string> setBuilderList = new(8);
        //[ProcessingRuns] ([Id], [StartedUtc], [TargetWidth], [TargetHeight], [Quality])
        if (run.Status != fromDb.Status)
            setBuilderList.Add($"[Status]={run.Status}");

        if (run.FinishedUtc != fromDb.FinishedUtc)
            setBuilderList.Add($"[FinishedUtc]={run.FinishedUtc?.ToString("yyyy-MM-dd HH:mm:ss.ff") ?? "null"}");

        if (run.TotalFiles != fromDb.TotalFiles)
            setBuilderList.Add($"[TotalFiles]={run.TotalFiles}");

        if (run.SuccessCount != fromDb.SuccessCount)
            setBuilderList.Add($"[SuccessCount]={run.SuccessCount}");

        if (run.FailedCount != fromDb.FailedCount)
            setBuilderList.Add($"[FailedCount]={run.FailedCount}");

        if (run.SkippedCount != fromDb.SkippedCount)
            setBuilderList.Add($"[SkippedCount]={run.SkippedCount}");

        string setInstruction = String.Join(", ", setBuilderList);

        string updateScript = $"""
                                 UPDATE [ProcessingRunItems]
                                 SET {setInstruction}
                                 WHERE [Id] = {run.RunId}
                              """;

        await db.ExecuteAsync(updateScript, run);
    }

    public async Task FinaliseRunAsync(ProcessingRun run, CancellationToken cancellationToken = default) 
        => await UpsertRunAsync(run, cancellationToken);
    
    public async Task<ProcessingRun?> GetRunAsync(string runId, CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();

        string selectScript = """
                         SELECT [Id], [RunId], [SourceHash], [FileName], [Status], [Message], [DurationMs]
                         FROM [ProcessingRunItems]
                         WHERE [Id] = @runId
                      """;
        ProcessingRun? fromDb = await db.QuerySingleOrDefaultAsync<ProcessingRun>(selectScript, new { runId });

        return fromDb;
    }

    public async Task<IReadOnlyList<ProcessingRun>> GetRecentRunsAsync(int take, CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();

        string queryString = """
                                SELECT [RunId], [StartedUtc], [FinishedUtc], [Status], [TotalFiles], [SuccessCount], [FailedCount], [SkippedCount]
                                FROM [ProcessingRuns]
                                ORDER BY [StartedUtc] DESC
                                LIMIT @take
                             """;

        IEnumerable<ProcessingRun>? rows = await db.QueryAsync<ProcessingRun>(queryString, new { take });

        List<ProcessingRun> result = rows?.ToList() ?? new();

        return result;
    }

    public async Task<IReadOnlyList<ProcessingRunItem>> GetRunItemsAsync(string runId, CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();

        string queryString = """
                                SELECT [Id], [RunId], [SourceHash], [FileName], [Status], [Message], [DurationMs], [CreatedUtc]
                                FROM [ProcessingRunItems]
                                WHERE [RunId]=@runId
                                ORDER BY [Id]
                             """;

        IEnumerable<ProcessingRunItem>? rows = await db.QueryAsync<ProcessingRunItem>(queryString, new { runId });
        List<ProcessingRunItem> result = rows?.ToList() ?? new();

        return result;
    }
}
