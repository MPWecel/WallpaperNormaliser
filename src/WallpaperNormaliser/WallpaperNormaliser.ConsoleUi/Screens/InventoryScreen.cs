using Spectre.Console;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Scan;

namespace WallpaperNormaliser.ConsoleUi.Screens;
public sealed class InventoryScreen(IInputScanner scanner, IWorkingDirectoryResolver paths)
{
    private readonly IInputScanner _scanner = scanner;
    private readonly IWorkingDirectoryResolver _paths = paths;

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
