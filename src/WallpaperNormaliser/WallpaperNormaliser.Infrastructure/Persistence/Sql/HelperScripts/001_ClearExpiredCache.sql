DELETE FROM [PreprocessCache]
WHERE [ExpiredUtc] < strftime('%Y-%m-%dT%H:%M:%SZ','now');
