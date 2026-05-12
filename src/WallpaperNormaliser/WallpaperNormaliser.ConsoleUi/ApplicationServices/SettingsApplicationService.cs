using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WallpaperNormaliser.ConsoleUi.ApplicationServices.Interfaces;
using WallpaperNormaliser.ConsoleUi.Models.ViewModels;
using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Settings;

namespace WallpaperNormaliser.ConsoleUi.ApplicationServices;
public sealed class SettingsApplicationService(ISettingsRepository settingsRepository) : ISettingsApplicationService
{
    private readonly ISettingsRepository _repository = settingsRepository;

    public async Task<SettingsViewModel> GetSettingsAsync()
    {
        AppSettings domainModel = await GetDomainAppSettingsAsync();
        SettingsViewModel result = SettingsViewModel.FromDomainEntity(domainModel);
        return result;
    }

    public async Task SaveSettingsAsync(SettingsViewModel toSave)
    {
        AppSettings fromSource = await GetDomainAppSettingsAsync();
        AppSettings toSource = toSave.ToDomainEntity(fromSource);
        await _repository.SaveAsync(toSource);
    }

    private async Task<AppSettings> GetDomainAppSettingsAsync() => await _repository.GetAsync().ConfigureAwait(false);
}
