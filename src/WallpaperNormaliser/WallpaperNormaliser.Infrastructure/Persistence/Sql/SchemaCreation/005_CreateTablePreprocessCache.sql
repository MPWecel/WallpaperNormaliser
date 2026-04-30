DROP TABLE IF EXISTS [PreprocessCache];
CREATE TABLE [PreprocessCache]
(
    [SourceHash] TEXT PRIMARY KEY,
    [Resolution] TEXT NOT NULL,
    [JpegQuality] INTEGER NOT NULL,
    [OutputBytes] BLOB NOT NULL,
    [CreatedUtc] TEXT NOT NULL,
    [ExpiresUtc] TEXT NOT NULL
);
