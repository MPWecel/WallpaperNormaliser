using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Spectre.Console;

using WallpaperNormaliser.ConsoleUi.Services;

namespace WallpaperNormaliser.ConsoleUi.Screens;
public sealed class DashboardScreen
{
    private readonly WorkingDirectoryResolver _paths;

    public DashboardScreen(WorkingDirectoryResolver resolver)
    {
        _paths = resolver;
    }

    public Task ShowAsync()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine(DashboardConstants.Title);

        string root = _paths.GetRoot();
        Table table = new();
        table.AddColumn(DashboardConstants.TableColumnHeader_Folders)
             .AddColumn(DashboardConstants.TableColumnHeader_Exists);
        table.AddRow(DashboardConstants.FolderInput, DirectoryExistsLabel(root, DashboardConstants.FolderInput))
             .AddRow(DashboardConstants.FolderOutput, DirectoryExistsLabel(root, DashboardConstants.FolderOutput))
             .AddRow(DashboardConstants.FolderManifest, DirectoryExistsLabel(root, DashboardConstants.FolderManifest));

        AnsiConsole.Write(table);
        Console.ReadKey(true);

        return Task.CompletedTask;
    }

    private string DirectoryExistsLabel(string rootDirectory, string folderName) 
        => Directory.Exists(Path.Combine(rootDirectory, folderName)) ? "Yes" : "No";
}

internal static class DashboardConstants
{
    internal const string Title = "[grey]Dashboard[/]";

    internal const string TableColumnHeader_Folders = "FOLDER";
    internal const string TableColumnHeader_Exists = "EXISTS";

    internal const string FolderInput = "INPUT";
    internal const string FolderOutput = "OUTPUT";
    internal const string FolderManifest = "MANIFEST";
}
