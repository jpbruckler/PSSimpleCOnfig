using System.Management.Automation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PSSimpleConfig.Utilities;

namespace PSSimpleConfig.Cmdlets;

[Cmdlet(VerbsData.Import, "PSSConfig")]
public class ImportPSSConfig : PSCmdlet
{
    [Parameter(Mandatory = true)]
    [Alias("Project")]
    public string Name { get; set; }

    protected override void ProcessRecord()
    {
        JObject config = PSSimpleConfig.ImportConfig(Name);
        SessionState.PSVariable.Set("PSSimpleConfig", JsonConversion.ToPSOutput(config));
        WriteObject(SessionState.PSVariable.Get("PSSimpleConfig"));
    }
}