using System.Text;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Logging;

namespace WallpaperNormaliser.Infrastructure.Logging;
public sealed class FileLogSink : ILogRepository
{
    private static readonly string LogDirectory =
        Path.Combine(AppContext.BaseDirectory, "logs");

    private static string TodayLogPath()
        => Path.Combine(LogDirectory, $"wallpaper-normaliser-{DateTimeOffset.UtcNow:yyyy-MM-dd}.log");

    private static string FormatEntry(LogEntry entry)
    {
        var sb = new StringBuilder();
        sb.Append($"[{entry.CreationDateUtc:yyyy-MM-dd HH:mm:ss} UTC] [{entry.Severity,-11}] [{entry.Category}] {entry.Message}");

        if (entry.CorrelationId is not null)
            sb.Append($" | CorrelationId={entry.CorrelationId}");

        if (entry.SourceHash is not null)
            sb.Append($" | SourceHash={entry.SourceHash}");

        if (entry.ExceptionMessage is not null)
            sb.Append($"{Environment.NewLine}  Exception: {entry.ExceptionMessage}");

        return sb.ToString();
    }

    private static void EnsureLogDirectoryExists()
    {
        if (!Directory.Exists(LogDirectory))
            Directory.CreateDirectory(LogDirectory);
    }

    public async Task WriteAsync(LogEntry entry, CancellationToken cancellationToken = default)
    {
        EnsureLogDirectoryExists();
        await File.AppendAllTextAsync(TodayLogPath(), FormatEntry(entry) + Environment.NewLine, cancellationToken);
    }

    public async Task WriteManyAsync(IEnumerable<LogEntry> entries, CancellationToken cancellationToken = default)
    {
        EnsureLogDirectoryExists();
        string content = string.Join(Environment.NewLine, entries.Select(FormatEntry)) + Environment.NewLine;
        await File.AppendAllTextAsync(TodayLogPath(), content, cancellationToken);
    }

    public Task<IReadOnlyList<LogEntry>> QueryAsync(LogQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<LogEntry>>(Array.Empty<LogEntry>());

    public Task<long> CountAsync(LogQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult(0L);

    public Task<int> CleanupAsync(LogRetentionPolicy policy, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(LogDirectory))
            return Task.FromResult(0);

        DateTime cutoff = DateTime.UtcNow.AddDays(-policy.MaxDays);
        int deleted = 0;

        foreach (string file in Directory.GetFiles(LogDirectory, "wallpaper-normaliser-*.log"))
        {
            if (File.GetLastWriteTimeUtc(file) < cutoff)
            {
                File.Delete(file);
                deleted++;
            }
        }

        return Task.FromResult(deleted);
    }
}
