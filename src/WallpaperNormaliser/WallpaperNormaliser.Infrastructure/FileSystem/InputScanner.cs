using System.Collections.Concurrent;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Events;
using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Core.Models.Scan;
using WallpaperNormaliser.Core.Models.Settings;

namespace WallpaperNormaliser.Infrastructure.FileSystem;
public sealed class InputScanner(ISettingsRepository settingsRepository) : IInputScanner, IDisposable
{
    private static readonly string[] SupportedExtensions =
    [
        ".jpg", ".jpeg", ".bmp", ".png", ".gif", ".tiff", ".tif", ".webp"
    ];

    private FileSystemWatcher?                              _watcher;
    private CancellationTokenSource?                        _watchCts;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _debouncers = new(StringComparer.OrdinalIgnoreCase);
    private int _debounceMs = 300;

    public event EventHandler<FileDiscoveredEventArgs>? FileDiscovered;
    public event EventHandler<FileChangedEventArgs>?   FileChanged;
    public event EventHandler<FileRemovedEventArgs>?   FileRemoved;

    public Task<ScanResult> ScanAsync(ScanOptions options, CancellationToken cancellationToken = default)
    {
        DateTime start = DateTime.UtcNow;

        if (!Directory.Exists(options.InputDirectory))
            return Task.FromResult(new ScanResult([], 0, 0, DateTime.UtcNow - start));

        SearchOption mode = options.IsRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        string[] allFiles = Directory.GetFiles(options.InputDirectory, "*", mode);
        List<ScanItem> items = [];

        foreach (string file in allFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string extension = Path.GetExtension(file);
            if (!IsSupportedExtension(extension))
                continue;

            FileInfo info    = new(file);
            string relative  = Path.GetRelativePath(options.InputDirectory, file);

            items.Add(new ScanItem(
                Path.GetFileName(file),
                relative,
                file,
                FileFormatInfo.FromExtension(extension)!,
                info.Length,
                info.LastWriteTimeUtc
            ));
        }

        ScanResult result = new(items, items.Count, allFiles.Length - items.Count, DateTime.UtcNow - start);
        return Task.FromResult(result);
    }

    public async Task StartWatchingAsync(ScanOptions options, CancellationToken cancellationToken = default)
    {
        await StopWatchingAsync(cancellationToken);

        AppSettings settings = await settingsRepository.GetAsync(cancellationToken);
        _debounceMs = settings.ScanSettings.DebounceMilliseconds;

        _watchCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _watcher = new FileSystemWatcher(options.InputDirectory)
        {
            IncludeSubdirectories = options.IsRecursive,
            NotifyFilter          = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents   = true
        };

        _watcher.Created += OnCreated;
        _watcher.Changed += OnChanged;
        _watcher.Deleted += OnDeleted;
        _watcher.Renamed += OnRenamed;
    }

    public Task StopWatchingAsync(CancellationToken cancellationToken = default)
    {
        _watchCts?.Cancel();
        _watchCts?.Dispose();
        _watchCts = null;

        if (_watcher is not null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Created -= OnCreated;
            _watcher.Changed -= OnChanged;
            _watcher.Deleted -= OnDeleted;
            _watcher.Renamed -= OnRenamed;
            _watcher.Dispose();
            _watcher = null;
        }

        foreach (CancellationTokenSource cts in _debouncers.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        _debouncers.Clear();

        return Task.CompletedTask;
    }

    public void Dispose() => StopWatchingAsync().GetAwaiter().GetResult();

    // --- FileSystemWatcher handlers ---

    private void OnCreated(object sender, FileSystemEventArgs e)
        => ScheduleDiscovery(e.FullPath, e.Name);

    private void OnChanged(object sender, FileSystemEventArgs e)
        => ScheduleChange(e.FullPath, e.Name);

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        if (!IsSupportedExtension(Path.GetExtension(e.Name ?? e.FullPath)))
            return;

        CancelDebounce(e.FullPath);
        FileRemoved?.Invoke(this, new FileRemovedEventArgs(Path.GetFileName(e.FullPath), e.FullPath));
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        if (IsSupportedExtension(Path.GetExtension(e.OldName ?? e.OldFullPath)))
        {
            CancelDebounce(e.OldFullPath);
            FileRemoved?.Invoke(this, new FileRemovedEventArgs(Path.GetFileName(e.OldFullPath), e.OldFullPath));
        }

        ScheduleDiscovery(e.FullPath, e.Name);
    }

    // --- Debounce helpers ---

    private void ScheduleDiscovery(string fullPath, string? name)
    {
        if (!IsSupportedExtension(Path.GetExtension(name ?? fullPath)))
            return;

        Debounce(fullPath, () =>
        {
            FileInfo info = new(fullPath);
            if (!info.Exists) return;

            ScanItem item = BuildScanItem(fullPath, name, info);
            FileDiscovered?.Invoke(this, new FileDiscoveredEventArgs(item));
        });
    }

    private void ScheduleChange(string fullPath, string? name)
    {
        if (!IsSupportedExtension(Path.GetExtension(name ?? fullPath)))
            return;

        Debounce(fullPath, () =>
        {
            FileInfo info = new(fullPath);
            if (!info.Exists) return;

            ScanItem item = BuildScanItem(fullPath, name, info);
            FileChanged?.Invoke(this, new FileChangedEventArgs(item));
        });
    }

    private void Debounce(string fullPath, Action raise)
    {
        if (_debouncers.TryGetValue(fullPath, out CancellationTokenSource? existing))
        {
            existing.Cancel();
            existing.Dispose();
        }

        CancellationTokenSource cts = new();
        _debouncers[fullPath] = cts;

        CancellationToken watchToken = _watchCts?.Token ?? CancellationToken.None;

        _ = Task.Delay(_debounceMs, cts.Token).ContinueWith(t =>
        {
            if (!t.IsCanceled && !watchToken.IsCancellationRequested)
            {
                _debouncers.TryRemove(fullPath, out _);
                raise();
            }
        }, TaskScheduler.Default);
    }

    private void CancelDebounce(string fullPath)
    {
        if (_debouncers.TryRemove(fullPath, out CancellationTokenSource? cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    // --- Utilities ---

    private static ScanItem BuildScanItem(string fullPath, string? name, FileInfo info)
    {
        string fileName = Path.GetFileName(fullPath);
        string ext      = Path.GetExtension(fullPath);
        return new ScanItem(
            fileName,
            name ?? fileName,
            fullPath,
            FileFormatInfo.FromExtension(ext)!,
            info.Length,
            info.LastWriteTimeUtc
        );
    }

    private static bool IsSupportedExtension(string ext)
        => SupportedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
}
