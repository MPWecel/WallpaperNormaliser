using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Enums;
using WallpaperNormaliser.Core.Models.Logging;

namespace WallpaperNormaliser.ConsoleUi.Screens;
public sealed class LogsScreen(ILogRepository logRepository)
{
    private readonly ILogRepository _logRepository = logRepository;

    public async Task ShowAsync()
    {
        AnsiConsole.Clear();

        string choice = AnsiConsole.Prompt(
                                              new SelectionPrompt<string>().Title("LogView:")
                                                                           .AddChoices(
                                                                                          "[1] Recent logs.",
                                                                                          "[2] Search logs.",
                                                                                          "[3] Back."
                                                                                      )
                                          );

        if (choice.Equals("Back", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        Table table = new();
        table.AddColumn("TIMESTAMP")
             .AddColumn("LEVEL")
             .AddColumn("MESSAGE");

        LogQuery query = new(null, null, ELogSeverity.Trace, null, null);

        IReadOnlyList<LogEntry> logs = await _logRepository.QueryAsync(query);

        foreach (var log in logs)
        {
            table.AddRow(
                            log.CreationDateUtc.ToString(),
                            log.Severity.ToString(),
                            log.Message
                        );
        }

        AnsiConsole.Write(table);

        Console.ReadKey();
    }
}
