using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallpaperNormaliser.ConsoleUi.Screens;

namespace WallpaperNormaliser.ConsoleUi.Navigation;
public sealed class MainMenu
{
    private readonly DashboardScreen _dashboard;
    private readonly ProcessingScreen _processing;
    private readonly InventoryScreen _inventory;
    private readonly SettingsScreen _settings;
    private readonly LogsScreen _logs;

    public MainMenu(DashboardScreen dashboard, ProcessingScreen processing, InventoryScreen inventory, SettingsScreen settings, LogsScreen logs)
    {
        _dashboard = dashboard;
        _processing = processing;
        _inventory = inventory;
        _settings = settings;
        _logs = logs;
    }

    public async Task ShowAsync()
    {
        while(true)
        {
            string choice = AnsiConsole.Prompt(
                                                  new SelectionPrompt<string>().Title(MainMenuConstants.MainMenuPrompt)
                                                                               .AddChoices(MainMenuConstants.MainMenuChoices)
                                              );

            switch(choice)
            {
                case MainMenuConstants.MainMenuChoice_RunProcessing:
                    await _processing.ShowAsync();
                    break;
                case MainMenuConstants.MainMenuChoice_Dashboard:
                    await _dashboard.ShowAsync();
                    break;
                case MainMenuConstants.MainMenuChoice_Inventory:
                    await _inventory.ShowAsync();
                    break;
                case MainMenuConstants.MainMenuChoice_Settings:
                    await _settings.ShowAsync();
                    break;
                case MainMenuConstants.MainMenuChoice_Logs:
                    await _logs.ShowAsync();
                    break;
                case MainMenuConstants.MainMenuChoice_Exit:
                    break;
            }


            await Task.CompletedTask;
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
    internal static string[] MainMenuChoices = [
                                                   MainMenuChoice_RunProcessing,
                                                   MainMenuChoice_Dashboard,
                                                   MainMenuChoice_Inventory,
                                                   MainMenuChoice_Settings,
                                                   MainMenuChoice_Logs,
                                                   MainMenuChoice_Exit
                                               ];
}
