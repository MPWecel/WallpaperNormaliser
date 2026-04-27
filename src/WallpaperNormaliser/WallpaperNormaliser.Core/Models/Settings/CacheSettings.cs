namespace WallpaperNormaliser.Core.Models.Settings;
public sealed record CacheSettings(
                                    bool IsEnabled,
                                    int MaxItems,
                                    int? ExpirationMinutes
                                  );
