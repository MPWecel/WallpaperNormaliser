using WallpaperNormaliser.Core.Models.Common;

namespace WallpaperNormaliser.Core.Models.Indexing;
public sealed record FileIndexEntry(
                                       string Id,
                                       string SourceHash,
                                       string RelativePath,
                                       string? FullPath,
                                       Resolution Resolution,
                                       long SizeBytes,
                                       DateTime LastSeenUtc,
                                       DateTime LastWriteUtc,
                                       bool IsDuplicate = false
                                   );
