namespace WallpaperNormaliser.Core.Models.Common;
public sealed record Resolution(int Width, int Height)
{
    public static Resolution? FromString(string input)
    {
        char separator = 'x';
        StringSplitOptions options = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;

        if(!input.Contains(separator)) 
            return null;

        string[] chunks = input.Split(separator, options);
        bool parseResult = Int32.TryParse(chunks[0], out int width) & 
                           Int32.TryParse(chunks[1], out int height);

        if(!parseResult)
            return null;

        return new Resolution(width, height);
    }

    public override string ToString() => $"{Width}x{Height}";
}
