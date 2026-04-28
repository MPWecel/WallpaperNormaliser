namespace WallpaperNormaliser.Core.Models.Manifest;
public sealed record ManifestDocument(
                                        Guid Id,
                                        string SourceHash,
                                        string SourceFileName,
                                        string RelativePath,
                                        DateTimeOffset CreationDateUtc,
                                        DateTimeOffset LastUpdateDateUtc,
                                        IReadOnlyList<ManifestResultEntry> Results
                                     );
