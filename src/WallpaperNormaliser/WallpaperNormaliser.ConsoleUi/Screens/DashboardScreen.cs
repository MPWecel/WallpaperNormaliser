using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Spectre.Console;

using WallpaperNormaliser.ConsoleUi.Models.ViewModels;
using WallpaperNormaliser.ConsoleUi.Services;

namespace WallpaperNormaliser.ConsoleUi.Screens;
public sealed class DashboardScreen(WorkingDirectoryResolver resolver)
{
    private readonly WorkingDirectoryResolver _paths = resolver;

    public Task ShowAsync()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine(DashboardConstants.Title);

        string root = _paths.GetRoot();

        DirectoryStatusViewModel[] statuses = new[]
        {
            BuildStatus(root, DashboardConstants.FolderInput),
            BuildStatus(root, DashboardConstants.FolderOutput),
            BuildStatus(root, DashboardConstants.FolderManifest),
        };

        Table table = new();
        table.AddColumn(DashboardConstants.TableColumnHeader_Folders)
             .AddColumn(DashboardConstants.TableColumnHeader_Exists)
             .AddColumn(DashboardConstants.TableColumnHeader_FileCount);

        foreach (DirectoryStatusViewModel status in statuses)
        {
            table.AddRow(
                            status.Name,
                            status.Exists ? DashboardConstants.LabelYes : DashboardConstants.LabelNo,
                            status.Exists ? status.FileCount.ToString() : DashboardConstants.LabelDash
                        );
        }

        AnsiConsole.Write(table);
        Console.ReadKey(true);

        return Task.CompletedTask;
    }

    private static DirectoryStatusViewModel BuildStatus(string rootDirectory, string folderName)
    {
        string path = Path.Combine(rootDirectory, folderName);
        bool exists = Directory.Exists(path);
        int count = exists ? Directory.EnumerateFiles(path).Count() : 0;
        return new DirectoryStatusViewModel(folderName, exists, count);
    }
}

internal static class DashboardConstants
{
    internal const string Title = "[grey]Dashboard[/]";

    internal const string TableColumnHeader_Folders   = "FOLDER";
    internal const string TableColumnHeader_Exists    = "EXISTS";
    internal const string TableColumnHeader_FileCount = "FILES";

    internal const string FolderInput    = "INPUT";
    internal const string FolderOutput   = "OUTPUT";
    internal const string FolderManifest = "MANIFEST";

    internal const string LabelYes  = "Yes";
    internal const string LabelNo   = "No";
    internal const string LabelDash = "-";
}
