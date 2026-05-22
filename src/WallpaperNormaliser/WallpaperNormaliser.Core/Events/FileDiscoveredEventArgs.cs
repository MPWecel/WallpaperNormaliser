using WallpaperNormaliser.Core.Models.Scan;

namespace WallpaperNormaliser.Core.Events;
public sealed class FileDiscoveredEventArgs(ScanItem item) : EventArgs
{
    public ScanItem Item { get; } = item;
}
