using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Core.Models.Processing;

namespace WallpaperNormaliser.Infrastructure.Imaging;
public class ImageSharpProcessor : IImageProcessor
{
    public Task<ImageProcessingResult> ProcessAsync(FileContext input, ProcessingOptions options, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
