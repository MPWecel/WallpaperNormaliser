using WallpaperNormaliser.Core.Enums;

namespace WallpaperNormaliser.Core.ValueObjects;
public sealed record FileFormatInfo(EFileFormat Format)
{
    public string Extension 
        => Format switch
        {
            EFileFormat.Jpeg    =>  "jpg",
            EFileFormat.Png     =>  "png",
            EFileFormat.Bmp     =>  "bmp",
            EFileFormat.Gif     =>  "gif",
            EFileFormat.Tiff    =>  "tiff",
            EFileFormat.Webp    =>  "webp",
            _ => "bin"
        };

    public string MimeType
        => Format switch
        {
            EFileFormat.Jpeg    =>  "image/jpeg",
            EFileFormat.Png     =>  "image/ong",
            EFileFormat.Bmp     =>  "image/bmp",
            EFileFormat.Gif     =>  "image/gif",
            EFileFormat.Tiff    =>  "image/tiff",
            EFileFormat.Webp    =>  "image/webp",
            _ => "application/octet-stream"
        };
}
