using WallpaperNormaliser.ConsoleUi.Models;
using WallpaperNormaliser.ConsoleUi.Models.ViewModels;

namespace WallpaperNormaliser.ConsoleUi.ApplicationServices.Interfaces;
public interface ISettingsApplicationService
{
    Task<SettingsViewModel> GetSettingsAsync();
    Task SaveSettingsAsync(SettingsViewModel toSave);
}
