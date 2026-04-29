using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Events;
using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Core.Models.Scan;

namespace WallpaperNormaliser.Infrastructure.FileSystem;
public class InputScanner : IInputScanner
{
    public event EventHandler<FileDiscoveredEventArgs>? FileDiscovered;
    public event EventHandler<FileChangedEventArgs>? FileChanged;
    public event EventHandler<FileRemovedEventArgs>? FileRemoved;

    public Task<ScanResult> ScanAsync(ScanOptions options, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task StartWatchingAsync(ScanOptions options, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task StopWatchingAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
