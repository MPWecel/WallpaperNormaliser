namespace WallpaperNormaliser.Core.Models.Common;
public sealed record FileContext(
                                    string FileName,
                                    string RelativePath,
                                    string? FullPath,
                                    byte[] Bytes,
                                    FileFormatInfo Format,
                                    string? Hash = null,
                                    IReadOnlyDictionary<string, string>? Metadata = null
                                );
