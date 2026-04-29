DROP TABLE IF EXISTS [FileIndex];
CREATE TABLE [FileIndex]
(
    [Id] TEXT PRIMARY KEY,
    [SourceHash] TEXT NOT NULL,
    [FileName] TEXT NOT NULL,
    [RelativePath] TEXT NOT NULL,
    [FullPath] TEXT,
    [Format] INTEGER NOT NULL,
    [SizeBytes] INTEGER NOT NULL,
    [Width] INTEGER,
    [Height] INTEGER,
    [LastSeenUtc] TEXT NOT NULL,
    [LastWriteUtc] TEXT,
    [IsDuplicate] INTEGER NOT NULL DEFAULT 0
);
