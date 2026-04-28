namespace WallpaperNormaliser.Core.Models.Logging;
public sealed record LogRetentionPolicy(
                                            int MaxDays,
                                            int MaxRows,
                                            int KeepDays,
                                            int KeepRows
                                       );
