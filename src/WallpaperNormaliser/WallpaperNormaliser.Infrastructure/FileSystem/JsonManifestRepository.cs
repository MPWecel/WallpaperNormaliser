using System.Text;
using System.Text.Json;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Manifest;

namespace WallpaperNormaliser.Infrastructure.FileSystem;
public sealed class JsonManifestRepository : IManifestRepository
{
    private readonly string _manifestDirectory;

    public JsonManifestRepository(string manifestDirectory)
    {
        _manifestDirectory = manifestDirectory;
        
        if(!Directory.Exists(_manifestDirectory))
            Directory.CreateDirectory(_manifestDirectory);
    }

    public JsonManifestRepository() : this(
                                              Path.GetFullPath(
                                                                  Path.Combine(
                                                                                  AppContext.BaseDirectory,
                                                                                  "..",
                                                                                  "..",
                                                                                  "..",
                                                                                  "..",
                                                                                  "APPLICATION_WORKING_DIRECTORY",
                                                                                  "MANIFEST"
                                                                              )
                                                              )
                                          ) { }

    public Task<ManifestDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();

    public async Task<ManifestDocument?> GetByFileNameAsync(string fileName, CancellationToken cancellationToken = default)
    {
        ManifestDocument? result = null;
        string prefix = SanitizeFileName(fileName);
        string? file = Directory.EnumerateFiles(_manifestDirectory, $"{prefix}_*.json").FirstOrDefault();

        if (!String.IsNullOrEmpty(file))
        {
            string? json = await File.ReadAllTextAsync(file, Encoding.Default, cancellationToken).ConfigureAwait(false);
            result = JsonSerializer.Deserialize<ManifestDocument>(json);
        }

        return result;
    }

    public async Task<ManifestDocument?> GetByHashAsync(string hash, CancellationToken cancellationToken = default)
    {
        ManifestDocument? result = null;
        string? file = Directory.EnumerateFiles(_manifestDirectory, $"*_{hash}.json").FirstOrDefault();

        if (!String.IsNullOrEmpty(file)) 
        {
            string? json = await File.ReadAllTextAsync(file, Encoding.Default, cancellationToken).ConfigureAwait(false);
            result = JsonSerializer.Deserialize<ManifestDocument>(json);
        }

        return result;
    }

    public async Task<IReadOnlyList<ManifestDocument>> GetManyAsync(ManifestQuery query, CancellationToken cancellationToken = default)
    {
        List<ManifestDocument> result = new();
        string fileName = String.IsNullOrEmpty(query.FileName) ? "*" : query.FileName;
        string hash = String.IsNullOrEmpty(query.SourceHash) ? "*" : query.SourceHash;
        EnumerationOptions options = new() { MatchCasing = MatchCasing.CaseInsensitive, IgnoreInaccessible = true};

        List<string> files = Directory.EnumerateFiles(_manifestDirectory, $"{fileName}_{hash}.json", options)
                                      .Take(query.Limit)
                                      .ToList();
        if (files.Any())
            foreach(var file in files)
            {
                string? json = await File.ReadAllTextAsync(file, Encoding.Default, cancellationToken).ConfigureAwait(false);
                ManifestDocument? doc = JsonSerializer.Deserialize<ManifestDocument>(json);
                
                if(doc is not null)
                    result.Add(doc);
            }

        return result;
    }

    public async Task SaveAsync(ManifestDocument document, CancellationToken cancellationToken = default)
    {
        string fileName = $"{SanitizeFileName(document.SourceFileName)}_{document.SourceHash}.json";
        string path = Path.Combine(_manifestDirectory, fileName);
        JsonSerializerOptions options = new() { WriteIndented = true };
        string json = JsonSerializer.Serialize(document, options);

        await File.WriteAllTextAsync(path, json, Encoding.Default, cancellationToken).ConfigureAwait(false);
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    private string SanitizeFileName(string fileName) => fileName.Replace('.', '_');
}
