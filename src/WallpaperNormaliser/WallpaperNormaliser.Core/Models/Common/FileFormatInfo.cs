using WallpaperNormaliser.Core.Enums;

namespace WallpaperNormaliser.Core.Models.Common;
public sealed record FileFormatInfo(EFileFormat Format)
{
    public string Extension => Format switch
                               {
                                   EFileFormat.Jpeg => "jpg",
                                   EFileFormat.Webp => "webp",
                                   EFileFormat.Tiff => "tiff",
                                   EFileFormat.Gif => "gif",
                                   EFileFormat.Bmp => "bmp",
                                   EFileFormat.Png => "png",
                                   _ => "bin"
                               };

    public string MimeType => Format switch
                              {
                                  EFileFormat.Jpeg => "image/jpeg",
                                  EFileFormat.Tiff => "image/tiff",
                                  EFileFormat.Webp => "image/webp",
                                  EFileFormat.Gif => "image/gif",
                                  EFileFormat.Bmp => "image/bmp",
                                  EFileFormat.Png => "image/png",
                                  _ => "application/octet-stream"
                              };

    private static readonly IReadOnlyDictionary<string, EFileFormat> _extensionMap =
        new Dictionary<string, EFileFormat>(StringComparer.OrdinalIgnoreCase)
        {
            ["jpg"] = EFileFormat.Jpeg,
            ["jpeg"] = EFileFormat.Jpeg,
            ["gif"] = EFileFormat.Gif,
            ["bmp"] = EFileFormat.Bmp,
            ["png"] = EFileFormat.Png,
            ["tif"] = EFileFormat.Tiff,
            ["tiff"] = EFileFormat.Tiff,
            ["webp"] = EFileFormat.Webp,
        };

    private static readonly IReadOnlyDictionary<string, EFileFormat> _mimeTypeMap =
        new Dictionary<string, EFileFormat>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpg"] = EFileFormat.Jpeg,
            ["image/jpeg"] = EFileFormat.Jpeg,
            ["image/gif"] = EFileFormat.Gif,
            ["image/bmp"] = EFileFormat.Bmp,
            ["image/png"] = EFileFormat.Png,
            ["image/tif"] = EFileFormat.Tiff,
            ["image/tiff"] = EFileFormat.Tiff,
            ["image/webp"] = EFileFormat.Webp,
        };

    /// <summary>Recognised file extensions, lower-case, without leading dot. Case-insensitive on lookup.</summary>
    public static IReadOnlySet<string> SupportedExtensions { get; } = new HashSet<string>(_extensionMap.Keys, StringComparer.OrdinalIgnoreCase);

    /// <summary>Recognised MIME types, lower-case. Case-insensitive on lookup.</summary>
    public static IReadOnlySet<string> SupportedMimeTypes { get; } = new HashSet<string>(_mimeTypeMap.Keys, StringComparer.OrdinalIgnoreCase);

    public static FileFormatInfo? FromExtension(string extension)
        => _extensionMap.TryGetValue(extension, out EFileFormat format) ? new FileFormatInfo(format) : null;

    public static FileFormatInfo? FromMimeType(string mimeType)
        => _mimeTypeMap.TryGetValue(mimeType, out EFileFormat format) ? new FileFormatInfo(format) : null;
}
