using WallpaperNormaliser.Core.Enums;
using WallpaperNormaliser.Core.Models.Common;

namespace WallpaperNormaliser.Core.Models.Processing;
public sealed record ImageProcessingResult(
                                            EProcessingStatus Status, 
                                            byte[]? OutputBytes,
                                            FileFormatInfo OutputFormat,
                                            Resolution OutputResolution,
                                            string? WarningMessage,
                                            string? ErrorMessage,
                                            TimeSpan Duration
                                          );
