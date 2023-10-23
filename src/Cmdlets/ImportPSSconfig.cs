
using System.IO;
using System.Management.Automation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PSSimpleConfig.Utilities;

namespace PSSimpleConfig.Cmdlets;

[Cmdlet(VerbsData.Import, "PSSConfig", DefaultParameterSetName = "Module")]
public class ImportPSSConfig : PSCmdlet
{
    [Parameter(Mandatory = true, ParameterSetName = "Module")]
    public string? ModuleName { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = "File")]
    public FileInfo ConfigFile { get; set; }
    protected override void ProcessRecord()
    {
        PSSC instance = PSSC.Instance; // Initialize the singleton instance

        if (ParameterSetName == "Module" && ModuleName is null)
        {
            throw new PSArgumentNullException(nameof(ModuleName));
        }
        else if (ParameterSetName == "File" && ConfigFile is null)
        {
            throw new PSArgumentNullException(nameof(ConfigFile));
        }

        if (ModuleName is not null)
        {
            PowerShell ps = PowerShell.Create();
            ps.AddCommand("Get-Module").AddParameter("Name", ModuleName).AddParameter("ListAvailable");
            var result = ps.Invoke();
            if (result.Count == 0)
            {
                throw new FileNotFoundException($"Unable to find module {ModuleName}");
            }
            WriteVerbose($"Initializing configuration for module {ModuleName} with base path {result[0].Properties["ModuleBase"].Value}");
            instance.Initialize(new DirectoryInfo(result[0].Properties["ModuleBase"].Value.ToString()));
        }
        else if (ConfigFile is not null)
        {
            WriteVerbose($"Initializing configuration for project {ConfigFile.Name}");
            instance.Initialize(ConfigFile);
        }
        else
        {
            WriteVerbose($"Initializing configuration for project using current directory {Path.Combine(Directory.GetCurrentDirectory(), "config", "config.json")}");
            instance.Initialize(new DirectoryInfo(Directory.GetCurrentDirectory()));
        }
        FileInfo _configPath = instance.ConfigPath;

        WriteVerbose($"Reading configuration from {_configPath.FullName}");
        WriteObject(JObject.Parse(File.ReadAllText(_configPath.FullName)));
    }
}