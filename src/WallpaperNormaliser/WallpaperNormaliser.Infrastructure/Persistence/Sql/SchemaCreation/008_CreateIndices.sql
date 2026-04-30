CREATE INDEX [IX_FileIndex_SourceHash]
ON [FileIndex]([SourceHash]);

CREATE INDEX [IX_FileIndex_LastSeenUtc]
ON [FileIndex]([LastSeenUtc]);

CREATE INDEX [IX_FileIndex_RelativePath]
ON [FileIndex]([RelativePath]);

CREATE INDEX [IX_RunItems_RunId]
ON [ProcessingRunItems]([RunId]);

CREATE INDEX [IX_RunItems_SourceHash]
ON [ProcessingRunItems]([SourceHash]);

CREATE INDEX [IX_Logs_CreatedUtc]
ON [Logs]([CreatedUtc]);

CREATE INDEX [IX_Logs_Severity]
ON [Logs]([Severity]);

CREATE INDEX [IX_Logs_CorrelationId]
ON [Logs]([CorrelationId]);

CREATE INDEX [IX_PreprocessCache_ExpiresUtc]
ON [PreprocessCache]([ExpiresUtc]);
