using WallpaperNormaliser.Core.Models.Common;

namespace WallpaperNormaliser.Core.Models.Cache;
public sealed record PreprocessCacheEntry(
                                            string SourceHash, 
                                            Resolution Resolution, 
                                            int Quality, 
                                            byte[] OutputBytes, 
                                            DateTime CreatedUtc, 
                                            DateTime ExpiresUtc
                                         );
