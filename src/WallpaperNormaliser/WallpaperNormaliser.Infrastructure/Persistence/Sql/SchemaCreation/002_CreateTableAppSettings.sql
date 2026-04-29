DROP TABLE IF EXISTS [AppSettings];
CREATE TABLE [AppSettings]
(
    [Key] TEXT PRIMARY KEY,
    [Value] TEXT NOT NULL,
    [UpdatedUtc] TEXT NOT NULL
);
