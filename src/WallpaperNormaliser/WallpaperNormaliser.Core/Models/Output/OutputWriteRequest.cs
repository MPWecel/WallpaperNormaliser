using WallpaperNormaliser.Core.Enums;

namespace WallpaperNormaliser.Core.Models.Output;
public sealed record OutputWriteRequest(
                                            string TargetDirectory,
                                            string FileName,
                                            byte[] Bytes,
                                            EOverwriteMode OverwriteMode
                                       );
