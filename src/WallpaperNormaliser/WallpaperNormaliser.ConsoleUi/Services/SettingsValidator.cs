using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WallpaperNormaliser.Core.Models.Settings;

namespace WallpaperNormaliser.ConsoleUi.Services;
public sealed class SettingsValidator
{
    public SettingsValidationResult Validate(AppSettings settings)
    {
        List<string> errors = new();

        if (string.IsNullOrWhiteSpace(settings.RootDirectory))
            errors.Add("RootDirectory must not be empty.");

        if (settings.Resolution.Width == 0 || settings.Resolution.Height == 0)
            errors.Add("Resolution width and height must be greater than 0.");

        if (settings.Quality < 1 || settings.Quality > 100)
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

        return new SettingsValidationResult(errors.Count == 0, errors);
    }
}
