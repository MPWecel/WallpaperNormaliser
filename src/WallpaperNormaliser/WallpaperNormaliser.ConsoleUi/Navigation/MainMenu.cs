using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallpaperNormaliser.ConsoleUi.Navigation;
public sealed class MainMenu
{
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
                    break;
                case MainMenuConstants.MainMenuChoice_Dashboard:
                    break;
                case MainMenuConstants.MainMenuChoice_Inventory:
                    break;
                case MainMenuConstants.MainMenuChoice_Settings:
                    break;
                case MainMenuConstants.MainMenuChoice_Logs:
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
