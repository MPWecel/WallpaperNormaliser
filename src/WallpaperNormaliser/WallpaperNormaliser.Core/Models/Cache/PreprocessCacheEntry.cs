using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallpaperNormaliser.Core.Models.Common;

namespace WallpaperNormaliser.Core.Models.Cache;
public sealed record PreprocessCacheEntry(
                                            string SourceHash, 
                                            Resolution Resolution, 
                                            int JpegQuality, 
                                            byte[] OutputBytes, 
                                            DateTime CreatedUtc, 
                                            DateTime ExpiresUtc
                                         );
