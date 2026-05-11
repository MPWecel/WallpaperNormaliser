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
public sealed class LogsScreen
{
    private readonly ILogRepository _logRepository;

    private readonly IReadOnlyDictionary<string, Func<Task>> _menuActionMappings;

    public LogsScreen(ILogRepository logRepository)
    {
        _logRepository = logRepository;

        _menuActionMappings = new Dictionary<string, Func<Task>>
        {
            [LogsScreenConstants.RecentLogs] = ShowRecentLogsAsync,
            [LogsScreenConstants.SearchLogs] = SearchLogsAsync
        };
    }

    public async Task ShowAsync()
    {
        while(true)
        {
            AnsiConsole.Clear();
            IReadOnlyList<LogEntry> logs = await _logRepository.QueryAsync(new(null, null, ELogSeverity.Trace, null, null));
            RenderLogsTable(logs);

            SelectionPrompt<string> prompt = new();
            prompt.Title(LogsScreenConstants.Prompt)
                  .AddChoices(LogsScreenConstants.Choices);

            string choice = AnsiConsole.Prompt(prompt);

            if (choice.Equals(LogsScreenConstants.Back, StringComparison.OrdinalIgnoreCase))
                return;

            bool isChoiceActionDefined = _menuActionMappings.TryGetValue(choice, out Func<Task>? action);

            if (!isChoiceActionDefined)
                continue;

            await action();
        }
    }

    private async Task ShowRecentLogsAsync()
    {
        LogQuery query = new(null, null, ELogSeverity.Trace, null, null, 0, 25);
        IReadOnlyList<LogEntry> logs = await _logRepository.QueryAsync(query);

        RenderLogsTable(logs);
        AnsiConsole.Console.Input.ReadKey(false);
    }

    private async Task SearchLogsAsync()
    {
        LogQuery query = new(null, null, ELogSeverity.Trace, null, null, 0, 25);
        IReadOnlyList<LogEntry> logs = await _logRepository.QueryAsync(query);

        RenderLogsTable(logs);
        AnsiConsole.Console.Input.ReadKey(false);
    }

    private static void RenderLogsTable(IEnumerable<LogEntry> logs)
    {
        Table table = new();

        table.AddColumn(LogsScreenConstants.LogsTableColumnHeader_Timestamp)
             .AddColumn(LogsScreenConstants.LogsTableColumnHeader_Category)
             .AddColumn(LogsScreenConstants.LogsTableColumnHeader_Severity)
             .AddColumn(LogsScreenConstants.LogsTableColumnHeader_Message)
             .AddColumn(LogsScreenConstants.LogsTableColumnHeader_ExceptionMessage);

        foreach (var log in logs.OrderByDescending(x=>x.CreationDateUtc))
        {
            table.AddRow(
                            log.CreationDateUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                            log.Category,
                            log.Severity.ToString(),
                            log.Message,
                            log.ExceptionMessage ?? String.Empty
                        );
        }

        AnsiConsole.Write(table);
    }
}

internal static class LogsScreenConstants
{
    internal const string Prompt = "Logs";

    internal const string LogsTableColumnHeader_Timestamp = "Timestamp";
    internal const string LogsTableColumnHeader_Category = "Category";
    internal const string LogsTableColumnHeader_Severity = "Severity";
    internal const string LogsTableColumnHeader_Message = "Message";
    internal const string LogsTableColumnHeader_ExceptionMessage = "ExceptionMessage";

    internal const string Search = "Search";

    internal const string RecentLogs = "[1] Recent logs";
    internal const string SearchLogs = "[2] Search logs";
    internal const string Back = "[0] Back";

    internal static readonly string[] Choices = [
                                                    RecentLogs,
                                                    SearchLogs,
                                                    Back
                                                ];
}
