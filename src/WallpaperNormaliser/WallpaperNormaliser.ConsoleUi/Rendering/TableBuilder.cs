using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;

namespace WallpaperNormaliser.ConsoleUi.Rendering;
public static class TableBuilder
{
    public static Table Create(params string[] columns)
    {
        Table table = new();

        foreach(var column in columns)
        {
            table.AddColumn(column);
        }

        return table;
    }
}
