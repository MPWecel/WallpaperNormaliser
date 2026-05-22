using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Logging;
using WallpaperNormaliser.Core.Models.Settings;
using WallpaperNormaliser.Infrastructure.Persistence.Repositories;

namespace WallpaperNormaliser.Infrastructure.Logging;
public sealed class CompositeLogger(SqliteLogRepository dbSink, FileLogSink fileSink, ISettingsRepository settingsRepository) : ILogRepository
{
    private LoggingSettings? _settings;

    private async ValueTask<LoggingSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        if (_settings is null)
        {
            AppSettings appSettings = await settingsRepository.GetAsync(cancellationToken);
            _settings = appSettings.LoggingSettings;
        }
        return _settings;
    }

    public async Task WriteAsync(LogEntry entry, CancellationToken cancellationToken = default)
    {
        LoggingSettings settings = await GetSettingsAsync(cancellationToken);
        List<Task> tasks = [];
        
        if (settings.IsDatabaseLoggingEnabled)
            tasks.Add(dbSink.WriteAsync(entry, cancellationToken));

        if (settings.IsFileLoggingEnabled)
            tasks.Add(fileSink.WriteAsync(entry, cancellationToken));

        await Task.WhenAll(tasks);
    }

    public async Task WriteManyAsync(IEnumerable<LogEntry> entries, CancellationToken cancellationToken = default)
    {
        LoggingSettings settings = await GetSettingsAsync(cancellationToken);
        IReadOnlyList<LogEntry> list = entries.ToList();
        List<Task> tasks = [];

        if (settings.IsDatabaseLoggingEnabled)
            tasks.Add(dbSink.WriteManyAsync(list, cancellationToken));

        if (settings.IsFileLoggingEnabled)
            tasks.Add(fileSink.WriteManyAsync(list, cancellationToken));
        
        await Task.WhenAll(tasks);
    }

    public async Task<IReadOnlyList<LogEntry>> QueryAsync(LogQuery query, CancellationToken cancellationToken = default)
    {
        LoggingSettings settings = await GetSettingsAsync(cancellationToken);
        
        if (settings.IsDatabaseLoggingEnabled)
            return await dbSink.QueryAsync(query, cancellationToken);
        
        return Array.Empty<LogEntry>().ToList();
    }

    public async Task<long> CountAsync(LogQuery query, CancellationToken cancellationToken = default)
    {
        LoggingSettings settings = await GetSettingsAsync(cancellationToken);
        long result = 0L;
        
        if (settings.IsDatabaseLoggingEnabled)
            result = await dbSink.CountAsync(query, cancellationToken);
        
        return result;
    }

    public async Task<int> CleanupAsync(LogRetentionPolicy policy, CancellationToken cancellationToken = default)
    {
        LoggingSettings settings = await GetSettingsAsync(cancellationToken);
        int total = 0;
        
        if (settings.IsDatabaseLoggingEnabled)
            total += await dbSink.CleanupAsync(policy, cancellationToken);
        
        if (settings.IsFileLoggingEnabled)
            total += await fileSink.CleanupAsync(policy, cancellationToken);
        
        return total;
    }
}
