using WallpaperNormaliser.Core.Events;
using WallpaperNormaliser.Core.Models.Scan;

namespace WallpaperNormaliser.Core.Contracts;
public interface IInputScanner
{
    event EventHandler<FileDiscoveredEventArgs>? FileDiscovered;
    event EventHandler<FileChangedEventArgs>? FileChanged;
    event EventHandler<FileRemovedEventArgs>? FileRemoved;

    Task<ScanResult> ScanAsync(ScanOptions options, CancellationToken cancellationToken = default);
    Task StartWatchingAsync(ScanOptions options, CancellationToken cancellationToken = default);
    Task StopWatchingAsync(CancellationToken cancellationToken = default);
}
