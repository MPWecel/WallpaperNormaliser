DROP TABLE IF EXISTS [ProcessingRunItems];
CREATE TABLE [ProcessingRunItems]
(
    [Id] TEXT PRIMARY KEY,
    [RunId] TEXT NOT NULL,
    [SourceHash] TEXT NULL,
    [FileName] TEXT NOT NULL,
    [Status] INTEGER NOT NULL,
    [Message] TEXT NULL,
    [DurationMs] INTEGER NULL,
    [CreatedUtc] TEXT NOT NULL
);
