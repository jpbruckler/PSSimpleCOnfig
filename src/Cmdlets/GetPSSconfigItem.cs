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
    public SwitchParameter AsDict { get; set; }

    protected override void ProcessRecord()
    {
        PSSC instance = PSSC.Instance; // Initialize the singleton instance

        JObject jObject;

        try
        {
            jObject = instance.ImportConfig(instance.ConfigPath);

            if (AsDict == true) {
                WriteObject(JsonConversion.ConvertToDictionary(jObject));
                return;
            }

            PSOutputWrapper output = Path != null
                                        ? JsonConversion.ToOutput(jObject.SelectToken(Path))
                                        : JsonConversion.ToOutput(jObject);

            switch (output.Type)
            {
                case PSOutputWrapper.OutputType.PSObject:
                    WriteVerbose($"Writing PSObject to pipeline");
                    WriteObject(output.PsObject);
                    break;
                case PSOutputWrapper.OutputType.Array:
                    WriteVerbose($"Writing Array to pipeline");
                    WriteObject(output.Array);
                    break;
                case PSOutputWrapper.OutputType.BasicType:
                    WriteVerbose($"Writing BasicType to pipeline");
                    WriteObject(output.BasicType);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown output type: {output.Type}");
            }

        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Unable to process query. Error: {e.Message}");
        }
    }
}