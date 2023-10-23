using System.IO;
using System.Management.Automation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PSSimpleConfig.Utilities;

namespace PSSimpleConfig.Cmdlets;

[Cmdlet(VerbsData.Import, "PSSConfig")]
public class ImportPSSConfig :PSCmdlet
{
    [Parameter(Mandatory = false)]
    public DirectoryInfo PathOverride { get; set; } = new DirectoryInfo(Directory.GetCurrentDirectory());
    protected override void ProcessRecord()
    {
        WriteVerbose($"Initializing configuration from {PathOverride.FullName}");
        FileInfo _configPath = PSSC.Initialize(PathOverride);

        WriteVerbose($"Reading configuration from {_configPath.FullName}");
        WriteObject(JObject.Parse(File.ReadAllText(_configPath.FullName)));
    }
}