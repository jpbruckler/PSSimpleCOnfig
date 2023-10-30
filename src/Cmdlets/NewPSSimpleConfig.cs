using System.IO;
using System.Management.Automation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PSSimpleConfig.Utilities;

namespace PSSimpleConfig.Cmdlets;

[Cmdlet(VerbsCommon.New, "PSSConfig", DefaultParameterSetName = "Path")]
public class NewPSSConfig : PSCmdlet
{
    [Parameter( Mandatory = false )]
    public FileInfo? Path { get; set; }

    protected override void ProcessRecord()
    {
        PSSC instance = PSSC.Instance; // Initialize the singleton instance

        // If -Path was given, use that, otherwise use ".\config\config.json"
        Path ??= new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "config", "config.json"));

        // PSSC.Instance.Initialize will create the directories if they don't exist
        // and updates the singleton.
        instance.Initialize(Path);
    }
}