using WallpaperNormaliser.Core.Models.Common;

namespace WallpaperNormaliser.Core.Models.Manifest;
public sealed record ManifestResultEntry(
                                            string FileName,
                                            Resolution Resolution,
                                            int Quality,
                                            string Hash,
                                            DateTimeOffset CreationDateUtc
                                        );
