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
    public FileInfo ConfigFile { get; set; }

    protected override void ProcessRecord()
    {
        PSSC instance = PSSC.Instance; // Initialize the singleton instance

        JObject jObject;
        _ = instance.ConfigPath;
        FileInfo configPath;
        if (ConfigFile is not null)
        {
            // Path was provided.
            configPath = ConfigFile;
        }
        else
        {
            // Path was not provided, use the default.
            configPath = instance.ConfigPath;
        }

        try
        {
            jObject = instance.ImportConfig(configPath);

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
            throw new InvalidOperationException($"Unable to import configuration file for project: {ConfigFile.Name}. Error: {e.Message}");
        }
    }
}