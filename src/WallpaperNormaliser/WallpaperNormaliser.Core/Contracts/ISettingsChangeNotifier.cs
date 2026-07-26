using WallpaperNormaliser.Core.Models.Settings;

namespace WallpaperNormaliser.Core.Contracts;
public interface ISettingsChangeNotifier
{
    event EventHandler<AppSettings> Changed;
    void Notify(AppSettings settings);
}
