CREATE TABLE IF NOT EXISTS [PreprocessCache]
(
    [SourceHash] TEXT NOT NULL,
    [Resolution] TEXT NOT NULL,
    [JpegQuality] INTEGER NOT NULL,
    [OutputBytes] BLOB NOT NULL,
    [CreatedUtc] TEXT NOT NULL,
    [ExpiresUtc] TEXT NOT NULL,
    PRIMARY KEY ([SourceHash], [Resolution], [JpegQuality])
);
