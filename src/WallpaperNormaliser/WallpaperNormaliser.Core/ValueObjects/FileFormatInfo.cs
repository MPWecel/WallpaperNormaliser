using System.Net.Http.Headers;
using WallpaperNormaliser.Core.Enums;

namespace WallpaperNormaliser.Core.ValueObjects;
public sealed record FileFormatInfo(EFileFormat Format)
{
    public string Extension 
        => Format switch
        {
            EFileFormat.Jpeg => "jpg",
            EFileFormat.Png  => "png",
            EFileFormat.Bmp  => "bmp",
            EFileFormat.Gif  => "gif",
            EFileFormat.Tiff => "tiff",
            EFileFormat.Webp => "webp",
            _ => "bin"
        };

    public string MimeType
        => Format switch
        {
            EFileFormat.Jpeg => "image/jpeg",
            EFileFormat.Png  => "image/png",
            EFileFormat.Bmp  => "image/bmp",
            EFileFormat.Gif  => "image/gif",
            EFileFormat.Tiff => "image/tiff",
            EFileFormat.Webp => "image/webp",
            _ => "application/octet-stream"
        };

    public static FileFormatInfo? FromExtension(string extension)
        => extension.ToLowerInvariant() switch
        {
            "jpg"  => new FileFormatInfo(EFileFormat.Jpeg),
            "jpeg" => new FileFormatInfo(EFileFormat.Jpeg),
            "gif"  => new FileFormatInfo(EFileFormat.Gif),
            "bmp"  => new FileFormatInfo(EFileFormat.Bmp),
            "png"  => new FileFormatInfo(EFileFormat.Png),
            "tif"  => new FileFormatInfo(EFileFormat.Tiff),
            "tiff" => new FileFormatInfo(EFileFormat.Tiff),
            "webp" => new FileFormatInfo(EFileFormat.Webp),
            _ => (FileFormatInfo?)(null)
        };

    public static FileFormatInfo? FromMimeType(string mimeType)
        => mimeType.ToLowerInvariant() switch
        {
            "image/jpg"  => new FileFormatInfo(EFileFormat.Jpeg),
            "image/jpeg" => new FileFormatInfo(EFileFormat.Jpeg),
            "image/gif"  => new FileFormatInfo(EFileFormat.Gif),
            "image/bmp"  => new FileFormatInfo(EFileFormat.Bmp),
            "image/png"  => new FileFormatInfo(EFileFormat.Png),
            "image/tif"  => new FileFormatInfo(EFileFormat.Tiff),
            "image/tiff" => new FileFormatInfo(EFileFormat.Tiff),
            "image/webp" => new FileFormatInfo(EFileFormat.Webp),
            _ => (FileFormatInfo?)(null)
        };
}
