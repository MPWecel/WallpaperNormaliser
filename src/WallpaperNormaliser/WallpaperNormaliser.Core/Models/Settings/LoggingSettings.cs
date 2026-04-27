namespace WallpaperNormaliser.Core.Models.Settings;
public sealed record LoggingSettings(
                                        bool IsFileLoggingEnabled,
                                        bool IsDatabaseLoggingEnabled,
                                        int RetentionDays,
                                        int MaxRows
                                    );
