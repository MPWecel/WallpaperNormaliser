using WallpaperNormaliser.Core.Enums;
using WallpaperNormaliser.Core.Models.Common;

namespace WallpaperNormaliser.Core.Models.Indexing;
public sealed record FileIndexEntry(
                                       string Id,
                                       string SourceHash,
                                       string FileName,
                                       string RelativePath,
                                       string? FullPath,
                                       EFileFormat Format,
                                       Resolution Resolution,
                                       long SizeBytes,
                                       DateTime LastSeenUtc,
                                       DateTime? LastWriteUtc,
                                       bool IsDuplicate = false
                                   );
