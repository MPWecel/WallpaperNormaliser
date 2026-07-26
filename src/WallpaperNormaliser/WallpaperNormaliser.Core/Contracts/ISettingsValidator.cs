using WallpaperNormaliser.Core.Models.Settings;

namespace WallpaperNormaliser.Core.Contracts;
public interface ISettingsValidator
{
    SettingsValidationResult Validate(AppSettings settings);
}
