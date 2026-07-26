namespace WallpaperNormaliser.Core.Exceptions;
public sealed class SettingsValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public SettingsValidationException(IReadOnlyList<string> errors)
        : base($"Settings validation failed: {string.Join("; ", errors)}")
    {
        Errors = errors;
    }
}
