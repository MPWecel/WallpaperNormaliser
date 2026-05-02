using WallpaperNormaliser.Core.Models.Common;

namespace WallpaperNormaliser.Core.Models.Settings;
public record AppSettings(
                             string RootDirectory,
                             Resolution Resolution,
                             int Quality,
                             ScanSettings ScanSettings,
                             CacheSettings CacheSettings,
                             LoggingSettings LoggingSettings
                         )
{
    public static AppSettings Default => new(
                                                "APPLICATION_WORKING_DIRECTORY",
                                                new(1920, 1080),
                                                100,
                                                new(true, true, 100),
                                                new(true, 1000, 60),
                                                new(true, true, 30, 2500)
                                            );
}
