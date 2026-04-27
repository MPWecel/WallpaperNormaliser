using WallpaperNormaliser.Core.Enums;
using WallpaperNormaliser.Core.ValueObjects;

namespace WallpaperNormaliser.Core.Models.Processing;
public sealed record ImageProcessingResult(
                                            EProcessingStatus Status, 
                                            byte[]? OutputBytes,
                                            FileFormatInfo OutputFormat,
                                            int OutputWidth,
                                            int OutputHeight,
                                            string? WarningMessage,
                                            string? ErrorMessage,
                                            TimeSpan Duration
                                          );
