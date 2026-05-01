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
                                 INSERT INTO [ProcessingRuns] ([Id], [StartedUtc], [TargetWidth], [TargetHeight], [Quality])
                                 VALUES (@Id, @StartedUtc, @TargetWidth, @TargetHeight, @Quality)
                              """;

        await db.ExecuteAsync(insertScript, run);
    }

    public async Task AddRunItemAsync(ProcessingRunItem item, CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();
        string insertScript = """
                                 INSERT INTO [ProcessingRunItems] ([Id], [RunId], [SourceHash], [FileName], [Status], [Message], [DurationMs])
                                 VALUES (@Id, @RunId, @SourceHash, @FileName, @Status, @Message, @DurationMs)
                              """;

        await db.ExecuteAsync(insertScript, item);
    }

    public async Task UpsertRunAsync(ProcessingRun run, CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();

        string selectScript = """
                                 SELECT [Id], [RunId], [SourceHash], [FileName], [Status], [Message], [DurationMs]
                                 FROM [ProcessingRunItems]
                                 WHERE [Id] = @RunId
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
            setBuilderList.Append($"[Status]={run.Status}, ");

        if (run.TotalFiles != fromDb.TotalFiles)
            setBuilderList.Append($"[TotalFiles]={run.TotalFiles}, ");

        

        string updateScript = $"""
                                 UPDATE [ProcessingRunItems]
                                 SET
                                 WHERE [Id] = @RunId
                                 VALUES (@Id, @RunId, @SourceHash, @FileName, @Status, @Message, @DurationMs)
                              """;

        await db.ExecuteAsync(updateScript, run);
    }

    public async Task FinaliseRunAsync(string runId)
    {

    }

}
