using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallpaperNormaliser.Core.Models.Scan;

namespace WallpaperNormaliser.ConsoleUi.Models.ViewModels;
public sealed record InventorySummaryViewModel(IReadOnlyList<InventoryItemViewModel> Items, int FilesFound, int FilesSkipped)
{
    public static InventorySummaryViewModel FromDomainModel(ScanResult input)
        => new(Items: input.Items.Select(InventoryItemViewModel.FromDomainEntity).ToList(), FilesFound: input.FilesFound, FilesSkipped: input.FilesSkipped);

    //public ScanResult ToDomainModel(ScanResult original)
    //{
    //    IList<ScanItem> originalScanItems = original.Items.ToList();
    //    IList<InventoryItemViewModel> inventoryItems = this.Items.ToList();
    //
    //
    //    ScanResult result = original with { Items = this.Items.Select(x=>x.ToDomainEntity()), FilesFound = this.FilesFound, FilesSkipped = this.FilesSkipped };
    //    return result;
    //}
}
