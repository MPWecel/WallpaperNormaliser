using WallpaperNormaliser.Core.Models.Common;

namespace WallpaperNormaliser.Core.Models.Settings;
public sealed record AppSettings(
                                    string RootDirectory,
                                    Resolution DefaultResolution,
                                    int DefaultJpegQuality,
                                    ScanSettings ScanSettings,
                                    CacheSettings CacheSettings,
                                    LoggingSettings LoggingSettings
                                );
