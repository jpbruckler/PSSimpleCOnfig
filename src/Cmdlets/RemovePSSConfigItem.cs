using System.Management.Automation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PSSimpleConfig.Cmdlets;

[Cmdlet(VerbsCommon.Remove, "PSSConfigItem")]
public class RmovePSSConfigItem : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0)]
    [Alias("Project")]
    public string Name { get; set; }

    [Parameter(Mandatory = true, Position = 1)]
    public string Path { get; set; }

    protected override void ProcessRecord()
    {

    }
}