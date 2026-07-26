using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Logging;
using WallpaperNormaliser.Core.Models.Settings;
using WallpaperNormaliser.Infrastructure.Persistence.Repositories;

namespace WallpaperNormaliser.Infrastructure.Logging;
public sealed class CompositeLogger : ILogRepository, IDisposable
{
    private readonly SqliteLogRepository _dbSink;
    private readonly FileLogSink _fileSink;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ISettingsChangeNotifier _changeNotifier;

    private LoggingSettings? _settings;

    public CompositeLogger(
                              SqliteLogRepository dbSink,
                              FileLogSink fileSink,
                              ISettingsRepository settingsRepository,
                              ISettingsChangeNotifier changeNotifier
                          )
    {
        _dbSink = dbSink;
        _fileSink = fileSink;
        _settingsRepository = settingsRepository;
        _changeNotifier = changeNotifier;

        _changeNotifier.Changed += OnSettingsChanged;
    }

    public void Dispose()
    {
        _changeNotifier.Changed -= OnSettingsChanged;
    }

    public async Task WriteAsync(LogEntry entry, CancellationToken cancellationToken = default)
    {
        LoggingSettings settings = await GetSettingsAsync(cancellationToken);
        List<Task> tasks = [];

        if (settings.IsDatabaseLoggingEnabled)
            tasks.Add(_dbSink.WriteAsync(entry, cancellationToken));

        if (settings.IsFileLoggingEnabled)
            tasks.Add(_fileSink.WriteAsync(entry, cancellationToken));

        await Task.WhenAll(tasks);
    }

    public async Task WriteManyAsync(IEnumerable<LogEntry> entries, CancellationToken cancellationToken = default)
    {
        LoggingSettings settings = await GetSettingsAsync(cancellationToken);
        IReadOnlyList<LogEntry> list = entries.ToList();
        List<Task> tasks = [];

        if (settings.IsDatabaseLoggingEnabled)
            tasks.Add(_dbSink.WriteManyAsync(list, cancellationToken));

        if (settings.IsFileLoggingEnabled)
            tasks.Add(_fileSink.WriteManyAsync(list, cancellationToken));

        await Task.WhenAll(tasks);
    }

    public async Task<IReadOnlyList<LogEntry>> QueryAsync(LogQuery query, CancellationToken cancellationToken = default)
    {
        LoggingSettings settings = await GetSettingsAsync(cancellationToken);

        if (settings.IsDatabaseLoggingEnabled)
            return await _dbSink.QueryAsync(query, cancellationToken);

        return Array.Empty<LogEntry>();
    }

    public async Task<long> CountAsync(LogQuery query, CancellationToken cancellationToken = default)
    {
        LoggingSettings settings = await GetSettingsAsync(cancellationToken);

        if (settings.IsDatabaseLoggingEnabled)
            return await _dbSink.CountAsync(query, cancellationToken);

        return 0L;
    }

    public async Task<int> CleanupAsync(LogRetentionPolicy policy, CancellationToken cancellationToken = default)
    {
        LoggingSettings settings = await GetSettingsAsync(cancellationToken);
        int total = 0;

        if (settings.IsDatabaseLoggingEnabled)
            total += await _dbSink.CleanupAsync(policy, cancellationToken);

        if (settings.IsFileLoggingEnabled)
            total += await _fileSink.CleanupAsync(policy, cancellationToken);

        return total;
    }

    private void OnSettingsChanged(object? sender, AppSettings settings)
        => Interlocked.Exchange(ref _settings, settings.LoggingSettings);

    private async ValueTask<LoggingSettings> GetSettingsAsync(CancellationToken cancellationToken)
    {
        LoggingSettings? snapshot = Volatile.Read(ref _settings);
        if (snapshot is not null)
            return snapshot;

        AppSettings appSettings = await _settingsRepository.GetAsync(cancellationToken);
        LoggingSettings fresh = appSettings.LoggingSettings;

        Interlocked.CompareExchange(ref _settings, fresh, null);
        return Volatile.Read(ref _settings) ?? fresh;
    }
}
