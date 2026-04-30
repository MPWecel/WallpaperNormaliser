using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
    public async Task<ImageProcessingResult> ProcessAsync(
                                                    FileContext input, 
                                                    ProcessingOptions options, 
                                                    CancellationToken cancellationToken = default
                                                   )
    {
        using Stream byteStream = new MemoryStream(input.Bytes, writable: false);
        using Image image = await Image.LoadAsync(byteStream, cancellationToken);

        image.Mutate(x => x.AutoOrient());

        double[] scaleAvailableValues =
                                        [
                                            1.0,
                                            ((double)(options.TargetResolution.Width / image.Width)),
                                            ((double)(options.TargetResolution.Height / image.Height))
                                        ];

        double scale = scaleAvailableValues.Min();

        int newWidth = Math.Max(1, ((int)(Math.Round(image.Width * scale))));
        int newHeight = Math.Max(1, ((int)(Math.Round(image.Height * scale))));

        image.Mutate(x => x.Resize(newWidth, newHeight));

        using Image<Rgba32> canvas = new(
                                            options.TargetResolution.Width, 
                                            options.TargetResolution.Height, 
                                            Color.Black
                                        );

        int posX = (canvas.Width - newWidth) / 2;
        int posY = (canvas.Height - newHeight) / 2;

        canvas.Mutate(x => x.DrawImage(image, new Point(posX, posY), 1f));

        using MemoryStream saveStream = new();

        await canvas.SaveAsJpegAsync(saveStream, new JpegEncoder() { Quality = options.JpegQuality }, cancellationToken);

        ImageProcessingResult result = new(
                                            EProcessingStatus.Completed, 
                                            saveStream.ToArray(), 
                                            new FileFormatInfo(EFileFormat.Jpeg), 
                                            options.TargetResolution.Width, 
                                            options.TargetResolution.Height, 
                                            null, 
                                            null, 
                                            TimeSpan.Zero
                                          );

        return result;
    }

    private double GetMinValue(IEnumerable<double> input) => input.Min();
}
