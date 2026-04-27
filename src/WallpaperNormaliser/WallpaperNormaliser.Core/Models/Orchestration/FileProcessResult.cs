using WallpaperNormaliser.Core.Enums;

namespace WallpaperNormaliser.Core.Models.Orchestration;
public sealed record FileProcessResult(
                                        string FileName,
                                        EProcessingStatus Status,
                                        string? Message
                                      );
