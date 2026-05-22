using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Events;
using WallpaperNormaliser.Core.Models.Logging;
using WallpaperNormaliser.Core.Models.Scan;
using WallpaperNormaliser.Core.Models.Settings;

namespace WallpaperNormaliser.Infrastructure.Background;
public sealed class WatcherService(
    IInputScanner scanner,
    ISettingsRepository settingsRepository,
    ILogRepository logRepository
) : IDisposable
{
    private const string LogCategory      = "WatcherService";
    private const string InputSubDirectory = "INPUT";

    private CancellationTokenSource? _cts;
    private bool _isRunning;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
            return;

        AppSettings settings = await settingsRepository.GetAsync(cancellationToken);

        if (!settings.ScanSettings.IsWatchEnabled)
            return;

        string inputDirectory = Path.Combine(AppContext.BaseDirectory, settings.RootDirectory, InputSubDirectory);

        if (!Directory.Exists(inputDirectory))
        {
            await LogAsync(Core.Enums.ELogSeverity.Warning,
                $"Watch mode skipped: INPUT directory does not exist at '{inputDirectory}'.", cancellationToken);
            return;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        scanner.FileDiscovered += OnFileDiscovered;
        scanner.FileChanged    += OnFileChanged;
        scanner.FileRemoved    += OnFileRemoved;

        ScanOptions options = new(inputDirectory, settings.ScanSettings.IsRecursive, IsRaiseEventsOn: true, IsComputeHashesOn: false);
        await scanner.StartWatchingAsync(options, _cts.Token);

        _isRunning = true;
        await LogAsync(Core.Enums.ELogSeverity.Information,
            $"Watch mode started on '{inputDirectory}'.", cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning)
            return;

        scanner.FileDiscovered -= OnFileDiscovered;
        scanner.FileChanged    -= OnFileChanged;
        scanner.FileRemoved    -= OnFileRemoved;

        await scanner.StopWatchingAsync(cancellationToken);

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        _isRunning = false;
        await LogAsync(Core.Enums.ELogSeverity.Information, "Watch mode stopped.", cancellationToken);
    }

    public void Dispose() => StopAsync().GetAwaiter().GetResult();

    // --- Event handlers ---

    private void OnFileDiscovered(object? sender, FileDiscoveredEventArgs e)
        => FireAndForget(LogAsync(Core.Enums.ELogSeverity.Information,
            $"File discovered: {e.Item.RelativePath} ({e.Item.SizeBytes} bytes)."));

    private void OnFileChanged(object? sender, FileChangedEventArgs e)
        => FireAndForget(LogAsync(Core.Enums.ELogSeverity.Information,
            $"File changed: {e.Item.RelativePath} ({e.Item.SizeBytes} bytes)."));

    private void OnFileRemoved(object? sender, FileRemovedEventArgs e)
        => FireAndForget(LogAsync(Core.Enums.ELogSeverity.Information,
            $"File removed: {e.FileName}."));

    // --- Helpers ---

    private Task LogAsync(Core.Enums.ELogSeverity severity, string message, CancellationToken ct = default)
    {
        LogEntry entry = new(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            severity,
            LogCategory,
            message,
            CorrelationId: null,
            SourceHash: null,
            ExceptionMessage: null
        );
        return logRepository.WriteAsync(entry, ct);
    }

    private static void FireAndForget(Task task)
        => task.ContinueWith(t => { }, TaskScheduler.Default);
}
