using Spectre.Console;
using WallpaperNormaliser.ConsoleUi.Screens;

namespace WallpaperNormaliser.ConsoleUi.Navigation;
public sealed class MainMenu
{
    private readonly DashboardScreen _dashboard;
    private readonly ProcessingScreen _processing;
    private readonly InventoryScreen _inventory;
    private readonly SettingsScreen _settings;
    private readonly LogsScreen _logs;

    private readonly IReadOnlyDictionary<string, Func<Task>> _menuChoiceMappings;

    public MainMenu(DashboardScreen dashboard, ProcessingScreen processing, InventoryScreen inventory, SettingsScreen settings, LogsScreen logs)
    {
        _dashboard = dashboard;
        _processing = processing;
        _inventory = inventory;
        _settings = settings;
        _logs = logs;

        _menuChoiceMappings = new Dictionary<string, Func<Task>>
        {
            [MainMenuConstants.MainMenuChoice_RunProcessing] = _processing.ShowAsync,
            [MainMenuConstants.MainMenuChoice_Dashboard] = _dashboard.ShowAsync,
            [MainMenuConstants.MainMenuChoice_Inventory] = _inventory.ShowAsync,
            [MainMenuConstants.MainMenuChoice_Settings] = _settings.ShowAsync,
            [MainMenuConstants.MainMenuChoice_Logs] = _logs.ShowAsync,
        };
    }

    public async Task ShowAsync()
    {
        SelectionPrompt<string> prompt = new();
        prompt.Title(MainMenuConstants.MainMenuPrompt).AddChoices(MainMenuConstants.MainMenuChoices);

        while (true)
        {
            string choice = AnsiConsole.Prompt(prompt);
            if (choice.Equals(MainMenuConstants.MainMenuChoice_Exit))
                return;

            bool isChoiceActionDefined = _menuChoiceMappings.TryGetValue(choice, out Func<Task>? action);
            if (!isChoiceActionDefined)
                continue;

            await action!();
        }    
    }
}

internal static class MainMenuConstants
{
    internal const string MainMenuPrompt = "Choose action:";

    internal const string MainMenuChoice_RunProcessing = "[1] Run processing.";
    internal const string MainMenuChoice_Dashboard = "[2] Dashboard.";
    internal const string MainMenuChoice_Inventory = "[3] Inventory.";
    internal const string MainMenuChoice_Settings = "[4] Settings.";
    internal const string MainMenuChoice_Logs = "[5] Logs.";
    internal const string MainMenuChoice_Exit = "[0] Exit.";
    
    internal static readonly string[] MainMenuChoices = [
                                                            MainMenuChoice_RunProcessing,
                                                            MainMenuChoice_Dashboard,
                                                            MainMenuChoice_Inventory,
                                                            MainMenuChoice_Settings,
                                                            MainMenuChoice_Logs,
                                                            MainMenuChoice_Exit
                                                        ];
}
