using System.IO;
using System.Management.Automation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PSSimpleConfig.Utilities;

namespace PSSimpleConfig.Cmdlets;

[Cmdlet(VerbsData.Import, "PSSConfig", DefaultParameterSetName = "Path")]
public class ImportPSSConfig : PSCmdlet
{
    [Parameter(Mandatory = true, ParameterSetName = "Module")]
    [ValidateNotNullOrEmpty()]
    public string ModuleName { get; set; }

    [Parameter(Mandatory = false, ParameterSetName = "Path")]
    public string Path { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter PassThru { get; set; }
    protected override void ProcessRecord()
    {
        PSSC instance = PSSC.Instance; // Initialize the singleton instance

        // If a module name is specified, get the ModuleBasePath
        // If a Path is specified, use that instead
        if (ParameterSetName == "Module" && ModuleName is not null)
        {
            try {
                // Try to get the ModuleBasePath for the given module. If it
                // fails, throw an exception - the user will need to set the
                // path manually.
                Path = System.IO.Path.Combine(PSSC.GetModuleBasePath(ModuleName).FullName, "config", "config.json");
            }
            catch (Exception e)
            {
                throw new IOException($"Unable to get module base path for module: {ModuleName}. Is the module in your PSModulePath? Error: {e.Message}");
            }
        }
        else if (ParameterSetName == "Path" && Path is not null)
        {
            // If a path is specified, use that instead
            if (!File.Exists(Path))
            {
                throw new FileNotFoundException($"Unable to find config file at path: {Path}");
            }
            else
            {
                instance.Initialize(new FileInfo(Path));
            }
        }

        // At this point, we should either have thrown an error, or have a valid
        // config path. Get the config path and read the config file.
        FileInfo configPath = instance.ConfigPath;
        WriteVerbose($"Reading configuration from {configPath.FullName}");

        if (PassThru)
        {
            WriteObject(JsonConversion.ToOutput(JObject.Parse(File.ReadAllText(configPath.FullName))).PsObject);
            return;
        }
    }
}