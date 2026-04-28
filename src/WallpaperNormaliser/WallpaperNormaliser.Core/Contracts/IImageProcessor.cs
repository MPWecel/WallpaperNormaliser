using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Core.Models.Processing;

namespace WallpaperNormaliser.Core.Contracts;
public interface IImageProcessor
{
    Task<ImageProcessingResult> ProcessAsync(FileContext input, ProcessingOptions options, CancellationToken cancellationToken = default);
}
