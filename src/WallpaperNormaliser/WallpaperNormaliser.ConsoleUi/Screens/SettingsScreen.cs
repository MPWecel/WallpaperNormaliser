using Spectre.Console;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Core.Models.Settings;


namespace WallpaperNormaliser.ConsoleUi.Screens;
public sealed class SettingsScreen
{
    private readonly ISettingsRepository _settingsRepository;

    private readonly IReadOnlyDictionary<string, Func<Task>> _menuActionMappings;

    public SettingsScreen(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
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

            AppSettings settings = await _settingsRepository.GetAsync();

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
        AppSettings settings = await _settingsRepository.GetAsync();

        uint width = AnsiConsole.Ask<uint>(SettingsScreenConstants.ChangeResolutionPromptWidth, settings.Resolution.Width);
        uint height = AnsiConsole.Ask<uint>(SettingsScreenConstants.ChangeResolutionPromptHeight, settings.Resolution.Height);
        AppSettings updated = settings with { Resolution = new Resolution(width, height) };
        await _settingsRepository.SaveAsync(updated);

        AnsiConsole.MarkupLine($"[green]{SettingsScreenConstants.ChangeResolutionSuccessMessage}{updated.Resolution.ToString()}[/]");
        AnsiConsole.Console.Input.ReadKey(false);
    }

    private async Task ChangeJpegQualityAsync()
    {
        AppSettings settings = await _settingsRepository.GetAsync();

        int quality = AnsiConsole.Ask<int>(SettingsScreenConstants.ChangeQualityPrompt, settings.Quality);
        AppSettings updated = settings with { Quality = quality };
        await _settingsRepository.SaveAsync(updated);

        AnsiConsole.MarkupLine($"[green]{SettingsScreenConstants.ChangeQualitySuccessMessage}{quality}[/]");
        AnsiConsole.Console.Input.ReadKey(false);
    }

    private static void RenderSettingsTable(AppSettings settings)
    {
        Table table = new();

        table.AddColumn(SettingsScreenConstants.Setting)
             .AddColumn(SettingsScreenConstants.Value);

        table.AddRow(SettingsScreenConstants.TableRowLabel_Resolution, settings.Resolution.ToString())
             .AddRow(SettingsScreenConstants.TableRowLabel_Quality, settings.Quality.ToString());

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
