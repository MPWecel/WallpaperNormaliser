DELETE FROM [Logs]
WHERE [CreatedUtc] < datetime('now','-90 day');
