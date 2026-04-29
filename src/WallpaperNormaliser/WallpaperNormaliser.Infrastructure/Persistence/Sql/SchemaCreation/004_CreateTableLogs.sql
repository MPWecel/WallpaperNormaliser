DROP TABLE IF EXISTS [Logs];
CREATE TABLE [Logs]
(
    [Id] TEXT PRIMARY KEY,
    [CreatedUtc] TEXT NOT NULL,
    [Severity] INTEGER NOT NULL,
    [Category] TEXT NOT NULL,
    [Message] TEXT NOT NULL,
    [SourceHash] TEXT NULL,
    [CorrelationId] TEXT NULL,
    [Exception] TEXT NULL
);
