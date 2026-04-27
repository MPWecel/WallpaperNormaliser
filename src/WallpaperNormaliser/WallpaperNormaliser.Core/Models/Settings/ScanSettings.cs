namespace WallpaperNormaliser.Core.Models.Settings;
public sealed record ScanSettings(
                                    bool IsRecursive,
                                    bool IsWatchEnabled,
                                    int DebounceMilliseconds
                                 );
