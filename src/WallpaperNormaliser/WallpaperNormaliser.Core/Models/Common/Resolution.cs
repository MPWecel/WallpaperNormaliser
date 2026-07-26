namespace WallpaperNormaliser.Core.Models.Common;
public sealed record Resolution(uint Width, uint Height)
{
    public static Resolution? FromString(string? input)
    {
        if (String.IsNullOrWhiteSpace(input))
            return null;

        const char separator = 'x';
        StringSplitOptions options = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;

        string[] chunks = input.Split(separator, options);
        if (chunks.Length != 2)
            return null;

        uint width  = 0;
        uint height = 0;
        bool parseResult = UInt32.TryParse(chunks[0], out width) &&
                           UInt32.TryParse(chunks[1], out height);

        if (!parseResult)
            return null;

        return new Resolution(width, height);
    }

    public override string ToString() => $"{Width}x{Height}";
}
