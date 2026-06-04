CREATE INDEX IF NOT EXISTS [IX_FileIndex_SourceHash]
ON [FileIndex]([SourceHash]);

CREATE INDEX IF NOT EXISTS [IX_FileIndex_LastSeenUtc]
ON [FileIndex]([LastSeenUtc]);

CREATE INDEX IF NOT EXISTS [IX_FileIndex_RelativePath]
ON [FileIndex]([RelativePath]);

CREATE INDEX IF NOT EXISTS [IX_RunItems_RunId]
ON [ProcessingRunItems]([RunId]);

CREATE INDEX IF NOT EXISTS [IX_RunItems_SourceHash]
ON [ProcessingRunItems]([SourceHash]);

CREATE INDEX IF NOT EXISTS [IX_Logs_CreatedUtc]
ON [Logs]([CreatedUtc]);

CREATE INDEX IF NOT EXISTS [IX_Logs_Severity]
ON [Logs]([Severity]);

CREATE INDEX IF NOT EXISTS [IX_Logs_CorrelationId]
ON [Logs]([CorrelationId]);

CREATE INDEX IF NOT EXISTS [IX_PreprocessCache_ExpiresUtc]
ON [PreprocessCache]([ExpiresUtc]);
