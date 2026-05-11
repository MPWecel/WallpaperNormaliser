using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallpaperNormaliser.ConsoleUi.Services;
public sealed class StartupValidator(WorkingDirectoryResolver workingDirectoryResolver)
{
    private readonly WorkingDirectoryResolver _workingDirectoryResolver = workingDirectoryResolver;

    public Task<StartupValidationResult> ValidateAsync()
    {
        List<string> errors = new();
        string rootDir = _workingDirectoryResolver.GetRoot();

        if (!Directory.Exists(rootDir))
            errors.Add($"Application working directory\t{rootDir}\tdoes not exist.");

        bool validationResult = errors.Any();

        StartupValidationResult result = new(validationResult, errors);

        return Task.FromResult(result);
    }
}
