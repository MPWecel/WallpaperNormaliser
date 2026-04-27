using WallpaperNormaliser.Core.ValueObjects;

namespace WallpaperNormaliser.Core.Models.Scan;
public sealed record ScanItem(
                                string FileName,
                                string RelativePath,
                                string? FullPath,
                                FileFormatInfo Format,
                                long SizeBytes,
                                DateTimeOffset LastWriteTimeUtc
                             );
