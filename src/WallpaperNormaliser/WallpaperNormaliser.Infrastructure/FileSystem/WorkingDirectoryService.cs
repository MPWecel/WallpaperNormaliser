using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallpaperNormaliser.Infrastructure.FileSystem;
public class WorkingDirectoryService
{
    private string _rootDirectoryPath = "repo-root/db";
    private string _workingDirectoryName = "APPLICATION_WORKING_DIRECTORY";
    private string _inputDirectoryName = "INPUT";
    private string _outputDirectoryName = "OUTPUT";
    private string _manifestDirectoryName = "MANIFEST";

    public string InputPath => $"{_rootDirectoryPath}/{_workingDirectoryName}/{_inputDirectoryName}";
    public string OutputPath => $"{_rootDirectoryPath}/{_workingDirectoryName}/{_outputDirectoryName}";
    public string ManifestPath => $"{_rootDirectoryPath}/{_workingDirectoryName}/{_manifestDirectoryName}";

}
