using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Core.Models.Settings;

namespace WallpaperNormaliser.Core.Validation;
public sealed class SettingsValidator : ISettingsValidator
{
    public SettingsValidationResult Validate(AppSettings settings)
    {
        List<string> errors = new();

        if (String.IsNullOrWhiteSpace(settings.RootDirectory))
            errors.Add("RootDirectory must not be empty.");

        if (IsResolutionInvalid(settings.Resolution))
            errors.Add("Resolution width and height must be greater than 0.");

        if (IsQualityOutOfRange(settings.Quality))
            errors.Add("Quality must be between 1 and 100.");

        if (settings.ScanSettings.DebounceMilliseconds < 0)
            errors.Add("ScanSettings.DebounceMilliseconds must be >= 0.");

        if (settings.CacheSettings.MaxItems < 0)
            errors.Add("CacheSettings.MaxItems must be >= 0.");

        if (settings.CacheSettings.ExpirationMinutes < 0)
            errors.Add("CacheSettings.ExpirationMinutes must be >= 0.");

        if (settings.LoggingSettings.RetentionDays <= 0)
            errors.Add("LoggingSettings.RetentionDays must be greater than 0.");

        if (settings.LoggingSettings.MaxRows <= 0)
            errors.Add("LoggingSettings.MaxRows must be greater than 0.");

        SettingsValidationResult result = new(errors.Count == 0, errors);
        return result;
    }

    private static bool IsResolutionInvalid(Resolution resolution) => (resolution.Height == 0 || resolution.Width == 0);

    private static bool IsQualityOutOfRange(int quality) => (quality < 1 || quality > 100);
}
