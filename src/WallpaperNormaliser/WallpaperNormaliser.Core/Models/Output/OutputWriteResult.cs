namespace WallpaperNormaliser.Core.Models.Output;
public sealed record OutputWriteResult(
                                        bool IsSuccess,
                                        string FullPath,
                                        string? ErrorMessage
                                      );
