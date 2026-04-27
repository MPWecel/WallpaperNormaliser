namespace WallpaperNormaliser.Core.Models.Scan;
public sealed record ScanResult(
                                    IReadOnlyList<ScanItem> Items,
                                    int FilesFound,
                                    int FilesSkipped,
                                    TimeSpan Duration
                               );
