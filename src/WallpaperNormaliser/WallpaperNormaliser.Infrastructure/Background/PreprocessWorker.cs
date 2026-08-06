using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Enums;
using WallpaperNormaliser.Core.Models.Cache;
using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Core.Models.Processing;
using WallpaperNormaliser.Core.Models.Scan;
using WallpaperNormaliser.Core.Models.Settings;

namespace WallpaperNormaliser.Infrastructure.Background;
public sealed class PreprocessWorker(
                                        IInputScanner scanner,
                                        IImageProcessor imageProcessor,
                                        IHashService hashService,
                                        IPreprocessCacheRepository cacheRepository,
                                        ISettingsRepository settingsRepository,
                                        IWorkingDirectoryResolver workingDirectoryResolver
                                    )
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        AppSettings settings = await settingsRepository.GetAsync(cancellationToken);

        if (!settings.CacheSettings.IsEnabled)
            return;

        await cacheRepository.CleanupExpiredAsync(cancellationToken);

        string inputDirectory = workingDirectoryResolver.GetInputDirectory();

        ScanOptions scanOptions = new(inputDirectory, settings.ScanSettings.IsRecursive, IsRaiseEventsOn: false, IsComputeHashesOn: false);
        ScanResult scan = await scanner.ScanAsync(scanOptions, cancellationToken);

        ProcessingOptions processingOptions = new(
                                                     settings.Resolution,
                                                     settings.Quality,
                                                     ApplyExifOrientation: true,
                                                     WarnOnSmallImages: false,
                                                     MinimumWidth: 640,
                                                     MinimumHeight: 480,
                                                     DryRun: false
                                                 );

        DateTime expiry = DateTime.UtcNow.AddMinutes(settings.CacheSettings.ExpirationMinutes ?? 60);

        foreach (ScanItem item in scan.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string path = item.FullPath ?? item.RelativePath;
            byte[] bytes = await File.ReadAllBytesAsync(path, cancellationToken);

            FileContext fileContext = new(item.FileName, item.RelativePath, item.FullPath, bytes, item.Format);
            string hash = await hashService.ComputeAsync(fileContext, cancellationToken);

            PreprocessCacheEntry? existing = await cacheRepository.GetAsync(hash, settings.Resolution, settings.Quality, cancellationToken);
            if (existing is not null)
                continue;

            fileContext = fileContext with { Hash = hash };

            ImageProcessingResult result = await imageProcessor.ProcessAsync(fileContext, processingOptions, cancellationToken);

            if (result.Status != EProcessingStatus.Completed || result.OutputBytes is null)
                continue;

            PreprocessCacheEntry entry = new(
                                                hash,
                                                settings.Resolution,
                                                settings.Quality,
                                                result.OutputBytes,
                                                DateTime.UtcNow,
                                                expiry
                                            );

            await cacheRepository.UpsertAsync(entry, cancellationToken);
        }
    }
}
