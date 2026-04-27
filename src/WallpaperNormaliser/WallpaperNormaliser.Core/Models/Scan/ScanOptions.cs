
namespace WallpaperNormaliser.Core.Models.Scan;

public sealed record ScanOptions(
                                    string InputDirectory,
                                    bool IsRecursive,
                                    bool IsRaiseEventsOn,
                                    bool IsComputHashesOn
                                );