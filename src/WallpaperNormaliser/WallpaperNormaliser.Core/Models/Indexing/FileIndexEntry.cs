using WallpaperNormaliser.Core.Models.Common;

namespace WallpaperNormaliser.Core.Models.Indexing;
public sealed record FileIndexEntry(
                                       string Id,
                                       string Hash,
                                       string RelativePath,
                                       string? FullPath,
                                       Resolution Resolution,
                                       long SizeBytes,
                                       DateTime LastSeenUtc
                                   );
