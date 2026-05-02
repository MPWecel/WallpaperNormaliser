DROP TABLE IF EXISTS [ProcessingRuns];
CREATE TABLE [ProcessingRuns]
(
    [Id] TEXT PRIMARY KEY,
    [StartedUtc] TEXT NOT NULL,
    [FinishedUtc] TEXT NULL,
    [Status] INTEGER NOT NULL,
    [TotalFiles] INTEGER NOT NULL,
    [SuccessCount] INTEGER NOT NULL,
    [FailedCount] INTEGER NOT NULL,
    [SkippedCount] INTEGER NOT NULL
);
