using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Settings;

namespace WallpaperNormaliser.Infrastructure.Persistence;
public sealed class SettingsChangeNotifier : ISettingsChangeNotifier
{
    public event EventHandler<AppSettings>? Changed;

    public void Notify(AppSettings settings)
    {
        EventHandler<AppSettings>? handler = Changed;
        handler?.Invoke(this, settings);
    }
}
