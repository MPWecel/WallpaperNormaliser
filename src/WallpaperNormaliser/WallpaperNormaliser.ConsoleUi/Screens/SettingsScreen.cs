using Spectre.Console;
using WallpaperNormaliser.ConsoleUi.ApplicationServices.Interfaces;
using WallpaperNormaliser.ConsoleUi.Models.ViewModels;
using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Core.Models.Settings;


namespace WallpaperNormaliser.ConsoleUi.Screens;
public sealed class SettingsScreen
{
    private readonly ISettingsApplicationService _settingsAppService;

    private readonly IReadOnlyDictionary<string, Func<Task>> _menuActionMappings;

    public SettingsScreen(ISettingsApplicationService settingsAppService)
    {
        _settingsAppService = settingsAppService;
        _menuActionMappings = new Dictionary<string, Func<Task>>()
        {
            [SettingsScreenConstants.ChangeResolution] = ChangeResolutionAsync,
            [SettingsScreenConstants.ChangeJpegQuality] = ChangeJpegQualityAsync
        };
    }

    public async Task ShowAsync()
    {
        while(true)
        {
            AnsiConsole.Clear();

            SettingsViewModel settings = await _settingsAppService.GetSettingsAsync();

            RenderSettingsTable(settings);

            SelectionPrompt<string> prompt = new();
            prompt.Title(SettingsScreenConstants.SettingsScreenMenuPrompt)
                  .AddChoices(SettingsScreenConstants.Choices);

            string choice = AnsiConsole.Prompt(prompt);

            if (choice.Equals(SettingsScreenConstants.Back, StringComparison.OrdinalIgnoreCase))
                return;

            bool isChoiceActionDefined = _menuActionMappings.TryGetValue(choice, out Func<Task>? action);

            if (!isChoiceActionDefined)
                continue;

            await action!();
        }
    }

    private async Task ChangeResolutionAsync()
    {
        SettingsViewModel settings = await _settingsAppService.GetSettingsAsync();

        uint width = AnsiConsole.Ask<uint>(SettingsScreenConstants.ChangeResolutionPromptWidth, settings.Width);
        uint height = AnsiConsole.Ask<uint>(SettingsScreenConstants.ChangeResolutionPromptHeight, settings.Height);
        SettingsViewModel updated = settings.CreateUpdatedViewModel(width, height);
        await _settingsAppService.SaveSettingsAsync(updated);

        AnsiConsole.MarkupLine($"[green]{SettingsScreenConstants.ChangeResolutionSuccessMessage}{updated.ResolutionString}[/]");
        AnsiConsole.Console.Input.ReadKey(false);
    }

    private async Task ChangeJpegQualityAsync()
    {
        SettingsViewModel settings = await _settingsAppService.GetSettingsAsync();

        int quality = AnsiConsole.Ask<int>(SettingsScreenConstants.ChangeQualityPrompt, settings.Quality);
        SettingsViewModel updated = settings.CreateUpdatedViewModel(quality:  quality);
        await _settingsAppService.SaveSettingsAsync(updated);

        AnsiConsole.MarkupLine($"[green]{SettingsScreenConstants.ChangeQualitySuccessMessage}{quality}[/]");
        AnsiConsole.Console.Input.ReadKey(false);
    }

    private static void RenderSettingsTable(SettingsViewModel input)
    {
        Table table = new();

        table.AddColumn(SettingsScreenConstants.Setting)
             .AddColumn(SettingsScreenConstants.Value);

        table.AddRow(SettingsScreenConstants.TableRowLabel_Resolution, input.ResolutionString)
             .AddRow(SettingsScreenConstants.TableRowLabel_Quality, input.Quality.ToString());

        AnsiConsole.Write(table);
    }
}

internal static class SettingsScreenConstants
{
    internal const string Title = "SETTINGS";

    internal const string SettingsScreenMenuPrompt = "Choose your action:\t";

    internal const string Setting = "SETTING";
    internal const string Value = "VALUE";

    internal const string ChangeResolutionPromptWidth = "Target width:\t";
    internal const string ChangeResolutionPromptHeight = "Target height:\t";
    internal const string ChangeResolutionSuccessMessage = "Saved resolution:\t";

    internal const string ChangeQualityPrompt = "New JPEG quality:\t";
    internal const string ChangeQualitySuccessMessage = "Saved JPEG quality:\t";


    internal const string TableRowLabel_Resolution = "Resolution";
    internal const string TableRowLabel_Quality = "Quality";

    internal const string ChangeResolution = "[1] Change resolution";
    internal const string ChangeJpegQuality = "[2] Change JPEG quality";
    internal const string Back = "[0] Back";

    internal static readonly string[] Choices = [
                                                    ChangeResolution,
                                                    ChangeJpegQuality,
                                                    Back
                                                ];
}
