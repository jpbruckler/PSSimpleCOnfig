using System.Management.Automation;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PSSimpleConfig.Utilities;

namespace PSSimpleConfig.Cmdlets;

[Cmdlet(VerbsCommon.Get, "PSSConfigItem")]
public class GetPSSConfig : PSCmdlet
{

    [Parameter(Mandatory = false, Position = 0)]
    public string? Path { get; set; }

    [Parameter(Mandatory = false)]
    public DirectoryInfo PathOverride { get; set; } = new DirectoryInfo(".");

    protected override void ProcessRecord()
    {
        _ = new PSObject();
        JObject jObject;

        try
        {
            jObject = PSSC.ImportConfig(PathOverride);

            PSOutputWrapper output = JsonConversion.ToOutput(jObject.SelectToken(Path));
            switch (output.Type)
            {
                case PSOutputWrapper.OutputType.PSObject:
                    WriteObject(output.PsObject);
                    break;
                case PSOutputWrapper.OutputType.Array:
                    WriteObject(output.Array);
                    break;
                case PSOutputWrapper.OutputType.BasicType:
                    WriteObject(output.BasicType);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown output type: {output.Type}");
            }

        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Unable to import configuration file for project: {PathOverride.Name}. Error: {e.Message}");
        }
    }
}