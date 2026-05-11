using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Enums;
using WallpaperNormaliser.Core.Models.Output;

namespace WallpaperNormaliser.Infrastructure.FileSystem;
public sealed class OutputWriter : IOutputWriter
{
    public async Task<OutputWriteResult> WriteAsync(OutputWriteRequest request, CancellationToken cancellationToken = default)
    {
        OutputWriteResult? result;
        try
        {
            if(!Directory.Exists(request.TargetDirectory))
                Directory.CreateDirectory(request.TargetDirectory);

            string filePath = Path.Combine(request.TargetDirectory, request.FileName);

            if(File.Exists(filePath))
            {
                switch(request.OverwriteMode)
                {
                    case EOverwriteMode.SkipAll:
                    case EOverwriteMode.Skip:
                        result = new(false, filePath, "File already exists!");
                        break;
                    case EOverwriteMode.SaveAllWithTimestamp:
                    case EOverwriteMode.SaveWithTimestamp:
                        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss.ff");
                        string fileExtension = Path.GetExtension(filePath);
                        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                        filePath = Path.Combine(request.TargetDirectory, $"{fileNameWithoutExtension}_{timestamp}.{fileExtension}");
                        result = new(true, filePath, $"File exists, saved with timestamp. New filename: {fileNameWithoutExtension}");
                        break;
                    case EOverwriteMode.ReplaceAll:
                    case EOverwriteMode.Replace:
                        result = new(true, filePath, "File exists, overwritten.");
                        break;
                    default:
                        result = null;
                        break;
                }
            }
            else
            {
                result = new(true, filePath, null);
            }

            if (result?.IsSuccess ?? throw new ArgumentOutOfRangeException(nameof(request.OverwriteMode), "Malformed request: Unknown overwrite mode!"))
            {
                string tempPath = $"{filePath}.tmp";
                await File.WriteAllBytesAsync(tempPath, request.Bytes, cancellationToken);

                File.Move(tempPath, filePath, overwrite: true);
            }
        }
        catch(Exception ex)
        {
            result = new(false, String.Empty, ex.Message);
        }
        return result;
    }

    public async Task<IReadOnlyList<OutputWriteResult>> WriteManyAsync(IEnumerable<OutputWriteRequest> requests, CancellationToken cancellationToken = default)
    {
        IEnumerable<Task<OutputWriteResult>> tasks = requests.Select(x=>WriteAsync(x, cancellationToken)).ToList();
        List<OutputWriteResult> taskResults = (await Task.WhenAll(tasks).ConfigureAwait(false)).ToList();
        return taskResults;
    }
}
