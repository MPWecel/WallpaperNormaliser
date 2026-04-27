using WallpaperNormaliser.Core.ValueObjects;

namespace WallpaperNormaliser.Core.Contexts;
public sealed record FileContext(
                                    string FileName,
                                    string RelativePath,
                                    string? FullPath,
                                    byte[] Bytes,
                                    FileFormatInfo Format,
                                    string? Hash = null,
                                    IReadOnlyDictionary<string, string>? Metadata = null
                                );
