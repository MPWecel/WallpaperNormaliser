using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models;
using WallpaperNormaliser.Core.Models.Orchestration;
using WallpaperNormaliser.Core.Models.Manifest;
using WallpaperNormaliser.Core.Models.Scan;
using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Core.Models.Processing;
using WallpaperNormaliser.Core.Models.Output;
using WallpaperNormaliser.Core.Enums;

namespace WallpaperNormaliser.Infrastructure.Processing;
public sealed class ProcessingOrchestrator : IProcessingOrchestrator
{
    private readonly IInputScanner _scanner;
    private readonly IHashService _hashService;
    private readonly IImageProcessor _imageProcessor;
    private readonly IOutputWriter _outputWriter;
    private readonly IManifestRepository _manifestRepository;

    public ProcessingOrchestrator(
                                    IInputScanner scanner, 
                                    IHashService hash, 
                                    IImageProcessor processor, 
                                    IOutputWriter writer, 
                                    IManifestRepository manifest
                                 )
    {
        _scanner = scanner;
        _hashService = hash;
        _imageProcessor = processor;
        _outputWriter = writer;
        _manifestRepository = manifest;
    }

    public async Task<BatchProcessResult> RunAsync(ProcessRequest request, CancellationToken cancellationToken = default)
    {
        ScanResult scan = await _scanner.ScanAsync( request.ScanOptions, cancellationToken).ConfigureAwait(false);
        List<FileProcessResult> results = new();

        foreach(var item in scan.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string path = item?.FullPath ?? item!.RelativePath;

            byte[] bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);

            FileContext file = new(item.FileName, item.RelativePath, item.FullPath, bytes, item.Format);

            var hashTask = _hashService.ComputeAsync(file, cancellationToken).ConfigureAwait(false);

            ImageProcessingResult processed = await _imageProcessor.ProcessAsync(file, request.ProcessingOptions, cancellationToken).ConfigureAwait(false);

            if(processed is { OutputBytes: not null})
            {
                string folder = Path.Combine("OUTPUT", item.FileName.Replace('.','_'));

                OutputWriteRequest writeRequest = new(folder, item.FileName, processed.OutputBytes, request.OverwriteMode);

                await _outputWriter.WriteAsync(writeRequest, cancellationToken);
            }

            var hash = await hashTask;
            file = file with { Hash = hash };

            FileProcessResult fileProcessResult = new(item.FileName, processed.Status, processed.ErrorMessage);

            results.Add(fileProcessResult);
        }

        BatchProcessResult batchProcessResult = new(
                                                       Guid.CreateVersion7().ToString(),
                                                       results,
                                                       results.Count(x=>x.Status.Equals(EProcessingStatus.Completed)),
                                                       results.Count(x=>x.Status.Equals(EProcessingStatus.Failed)),
                                                       results.Count(x=>x.Status.Equals(EProcessingStatus.Skipped)),
                                                       TimeSpan.Zero
                                                   );
        return batchProcessResult;
    }
}
