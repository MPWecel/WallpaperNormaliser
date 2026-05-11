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
public sealed class InputScanner : IInputScanner
{
    private readonly string[] _supportedExtensions = [
                                                        ".jpg",
                                                        ".jpeg",
                                                        ".bmp",
                                                        ".png",
                                                        ".gif",
                                                        ".tiff",
                                                        ".webp"
                                                     ];

    public event EventHandler<FileDiscoveredEventArgs>? FileDiscovered;
    public event EventHandler<FileChangedEventArgs>? FileChanged;
    public event EventHandler<FileRemovedEventArgs>? FileRemoved;

    public Task<ScanResult> ScanAsync(ScanOptions options, CancellationToken cancellationToken = default)
    {
        ScanResult? result;
        DateTime scanStartTimestampUtc = DateTime.UtcNow;
        DateTime scanStopTimestampUtc;
        TimeSpan scantime;

        if (!Directory.Exists(options.InputDirectory))
        {
            scanStopTimestampUtc = DateTime.UtcNow;
            scantime = scanStopTimestampUtc - scanStartTimestampUtc;
            result = new(new List<ScanItem>(), 0, 0, scantime);
            return Task.FromResult(result);
        }

        SearchOption mode = options.IsRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        IEnumerable<string> files = Directory.GetFiles(options.InputDirectory, "*", mode);
        List<ScanItem> items = new();

        foreach (string file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string extension = Path.GetExtension(file);

            if (!(_supportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)))
                continue;

            FileInfo info = new(file);

            string relative = Path.GetRelativePath(options.InputDirectory, file);

            ScanItem item = new(
                                   Path.GetFileName(file), 
                                   relative, 
                                   file, 
                                   FileFormatInfo.FromExtension(extension)!,
                                   info.Length, 
                                   info.LastWriteTimeUtc
                               );
            items.Add(item);




        }

        scanStopTimestampUtc = DateTime.UtcNow;
        scantime = scanStopTimestampUtc - scanStartTimestampUtc;

        result = new(
                        items, 
                        items.Count, 
                        (files.Count() - items.Count), 
                        scantime
                    );

        return Task.FromResult(result);
    }

    public Task StartWatchingAsync(ScanOptions options, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task StopWatchingAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
