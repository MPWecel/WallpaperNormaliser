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
        try
        {
            using Stream byteStream = new MemoryStream(input.Bytes, writable: false);
            using Image image = await Image.LoadAsync<Rgba32>(byteStream, cancellationToken);

            string? validationMessage = ValidateMinimumSize(image.Width, image.Height);

            image.Mutate(x => x.AutoOrient());

            double[] scaleAvailableValues = [
                                                1.0,
                                                CalculateScaling(options.TargetResolution.Width, image.Width),
                                                CalculateScaling(options.TargetResolution.Height, image.Height)
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
            Point startingPoint = new(posX, posY);

            canvas.Mutate(x => x.DrawImage(image, startingPoint, 1f));

            using MemoryStream saveStream = new();
            JpegEncoder encoder = new() { Quality = options.JpegQuality };

            await canvas.SaveAsJpegAsync(saveStream, encoder, cancellationToken);
            TimeSpan duration = GetDuration(startDateUtc);
            FileFormatInfo targetFileFormat = new(EFileFormat.Jpeg);
            Resolution targetResolution = new(options.TargetResolution.Width, options.TargetResolution.Height);

            ImageProcessingResult result = new(
                                                EProcessingStatus.Completed,
                                                saveStream.ToArray(),
                                                targetFileFormat,
                                                targetResolution,
                                                validationMessage,
                                                null,
                                                duration
                                              );

            return result;
        }
        catch (Exception ex) when (IsHandledImageException(ex))
        {
            TimeSpan duration = GetDuration(startDateUtc);
            FileFormatInfo errorFileFormat = new(EFileFormat.Unknown);
            Resolution nullResolution = new(0,0);
            string exceptionMessage = $"{ex.GetType().Name}:\t{ex.Message}";

            ImageProcessingResult result = new(
                                                  EProcessingStatus.Failed,
                                                  null,
                                                  errorFileFormat,
                                                  nullResolution,
                                                  null,
                                                  exceptionMessage,
                                                  duration
                                              );

            return result;
        }
    }

    private static TimeSpan GetDuration(DateTime startDateUtc) => (DateTime.UtcNow - startDateUtc);

    private static string? ValidateMinimumSize(int width, int height)
        => CheckSize(width, height) ? ($"Image resolution may be too low!\tInput resolution: {width}x{height}.\tRecommended minimum resolution: 640x480") : (null);

    private static bool CheckSize(int width, int height) => (width < 640 || height < 480);

    private static double CalculateScaling(uint targetValue, int originalValue)
        => (((double)(targetValue)) / ((double)(originalValue)));

    private static double GetMinValue(IEnumerable<double> input) => input.Min();
    
    private static bool IsHandledImageException(Exception exception)
        => exception is IOException
                     or ImageFormatException
                     or UnknownImageFormatException;
}
