using SixLabors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Enums;
using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Core.Models.Processing;

namespace WallpaperNormaliser.Infrastructure.Imaging;
public class ImageSharpProcessor : IImageProcessor
{
    public async Task<ImageProcessingResult> ProcessAsync(FileContext input, ProcessingOptions options, CancellationToken cancellationToken = default)
    {
        DateTime startDateUtc = DateTime.UtcNow;
        using Stream byteStream = new MemoryStream(input.Bytes, writable: false);
        using Image image = await Image.LoadAsync<Rgba32>(byteStream, cancellationToken);

        string? validationMessage = ValidateMinimumSize(image.Width, image.Height);

        image.Mutate(x => x.AutoOrient());

        double[] scaleAvailableValues = [
                                            1.0,
                                            ((double)(options.TargetResolution.Width / image.Width)),
                                            ((double)(options.TargetResolution.Height / image.Height))
                                        ];

        double scale = GetMinValue(scaleAvailableValues);

        int newWidth = Math.Max(1, ((int)(Math.Round(image.Width * scale))));
        int newHeight = Math.Max(1, ((int)(Math.Round(image.Height * scale))));

        image.Mutate(x => x.Resize(newWidth, newHeight));

        using Image<Rgba32> canvas = new(
                                            (int)(options.TargetResolution.Width), 
                                            (int)(options.TargetResolution.Height), 
                                            Color.Black
                                        );

        int posX = (canvas.Width - newWidth) / 2;
        int posY = (canvas.Height - newHeight) / 2;

        canvas.Mutate(x => x.DrawImage(image, new Point(posX, posY), 1f));

        using MemoryStream saveStream = new();

        await canvas.SaveAsJpegAsync(saveStream, new JpegEncoder() { Quality = options.JpegQuality }, cancellationToken);
        DateTime finishedUtc = DateTime.UtcNow;
        TimeSpan duration = (finishedUtc - startDateUtc);

        ImageProcessingResult result = new(
                                            EProcessingStatus.Completed, 
                                            saveStream.ToArray(), 
                                            new FileFormatInfo(EFileFormat.Jpeg),
                                            new Resolution(options.TargetResolution.Width, options.TargetResolution.Height), 
                                            validationMessage, 
                                            null, 
                                            duration
                                          );

        return result;
    }

    private static double GetMinValue(IEnumerable<double> input) => input.Min();

    private static string? ValidateMinimumSize(int width, int height)
        => (width < 640 || height < 480) ?
                                         ($"Image resolution may be too low!\tInput resolution: {width}x{height}.\tRecommended minimum resolution: 640x480") :
                                         (null);
}
