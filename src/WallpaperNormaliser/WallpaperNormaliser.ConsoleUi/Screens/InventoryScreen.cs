using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Spectre.Console;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.ConsoleUi.Services;
using WallpaperNormaliser.Core.Models.Scan;

namespace WallpaperNormaliser.ConsoleUi.Screens;
public sealed class InventoryScreen
{
    private readonly IInputScanner _scanner;
    private readonly WorkingDirectoryResolver _paths;

    public InventoryScreen(IInputScanner scanner, WorkingDirectoryResolver paths)
    {
        _scanner = scanner;
        _paths = paths;
    }

    public async Task ShowAsync()
    {
        AnsiConsole.Clear();

        string inputDir = _paths.GetInputDirectory();

        ScanOptions options = new(inputDir,true, false, true);
        ScanResult scanResult = await _scanner.ScanAsync(options);
        int count = scanResult.FilesFound;

        AnsiConsole.MarkupLine(InventoryConstants.Title);
        AnsiConsole.MarkupLine($"{InventoryConstants.FilesDiscovered}{count}[/]");

        Console.ReadKey(true);
    }


}

internal static class InventoryConstants
{
    internal const string Title = "[grey]INVENTORY [/]";

    internal const string FilesDiscovered = "Files discovered:\t[green]";
}
