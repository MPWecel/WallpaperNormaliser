using WallpaperNormaliser.Core.Models.Common;

namespace WallpaperNormaliser.Core.Models.Processing;
public sealed record ProcessingOptions(
                                        Resolution TargetResolution, 
                                        int JpegQuality,
                                        bool ApplyExifOrientation,
                                        bool WarnOnSmallImages,
                                        int MinimumWidth,
                                        int MinimumHeight,
                                        bool DryRun
                                      );
