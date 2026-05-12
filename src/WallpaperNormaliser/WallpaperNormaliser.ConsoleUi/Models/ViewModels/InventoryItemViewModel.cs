using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Core.Models.Scan;

namespace WallpaperNormaliser.ConsoleUi.Models.ViewModels;
public sealed record InventoryItemViewModel(string FileName, string Extension)
{
    public static InventoryItemViewModel FromDomainEntity(ScanItem scanItem) 
        => new(FileName: scanItem.FileName, Extension: scanItem.Format.Extension);

    public ScanItem ToDomainEntity(ScanItem original)
    {
        ScanItem result = original with
        {
            FileName = this.FileName,
            Format = FileFormatInfo.FromExtension(this.Extension)
        };
        return result;
    }
}
