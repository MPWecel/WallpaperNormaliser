namespace WallpaperNormaliser.ConsoleUi.Models.ViewModels;
public sealed record DashboardSummaryViewModel(
                                                  IReadOnlyList<DirectoryStatusViewModel> DirectoryStatuses, 
                                                  int FilesProcessed, 
                                                  int FilesSkipped, 
                                                  int FilesFailed
                                              );
