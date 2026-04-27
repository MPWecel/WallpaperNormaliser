using WallpaperNormaliser.Core.Enums;
using WallpaperNormaliser.Core.Models.Processing;
using WallpaperNormaliser.Core.Models.Scan;

namespace WallpaperNormaliser.Core.Models.Orchestration;
public sealed record ProcessRequest(
                                        ScanOptions ScanOptions,
                                        ProcessingOptions ProcessingOptions,
                                        EOverwriteMode OverwriteMode
                                   );
