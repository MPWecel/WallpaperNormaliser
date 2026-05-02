using WallpaperNormaliser.Core.Models.Logging;

namespace WallpaperNormaliser.Core.Contracts;
public interface ILogRepository
{
    Task<IReadOnlyList<LogEntry>> QueryAsync(LogQuery query, CancellationToken cancellationToken = default);
    Task WriteAsync(LogEntry entry, CancellationToken cancellationToken = default);
    Task WriteManyAsync(IEnumerable<LogEntry> entries, CancellationToken cancellationToken = default);
    Task<int> CleanupAsync(LogRetentionPolicy policy, CancellationToken cancellationToken = default);
    Task<long> CountAsync(LogQuery query, CancellationToken cancellationToken= default);
}
