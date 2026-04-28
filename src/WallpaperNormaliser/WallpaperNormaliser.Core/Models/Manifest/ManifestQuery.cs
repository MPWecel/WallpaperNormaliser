namespace WallpaperNormaliser.Core.Models.Manifest;
public sealed record ManifestQuery(
                                    string? SourceHash,
                                    string? FileName,
                                    int Limit = 100
                                  );
