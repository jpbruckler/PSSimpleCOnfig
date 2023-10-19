using System.Collections;
using System.Management.Automation;
using Newtonsoft.Json;

namespace PSSimpleConfig.Cmdlets;

[Cmdlet(VerbsCommon.Set, "PSSConfig")]
public class SetPSSConfig : PSCmdlet
{
    [Parameter(Mandatory = false, Position = 0)]
    [Alias("Path")]
    public string? ConfigPath { get; set; }

    [Parameter(Mandatory = false, Position = 1)]
    public object? Value { get; set; }

    [Parameter(Mandatory = false)]
    [Alias("Project")]
    public string Name { get; set; } = "Default";

    [Parameter(Mandatory = false)]
    public Hashtable? Hashtable { get; set; }


    protected override void ProcessRecord()
    {
        // TODO: Load existing JSON config into a JObject

        // TODO: Parse the Path variable to set the value in JObject

        // TODO: Serialize the JObject back to JSON and save it

        // TODO: Handle errors and edge cases

        string configFilePath = Path.Combine(PSSimpleConfig.Namespaces, Name, "config.json");

        // Serialize the hashtable to a JSON string
        string json = JsonConvert.SerializeObject(Hashtable);

        try
        {
            File.WriteAllText(configFilePath, json);
            WriteVerbose($"Successfully wrote config to {configFilePath}");
        }
        catch (Exception ex)
        {
            WriteError(new ErrorRecord(ex, "FailedToWriteConfig", ErrorCategory.WriteError, null));
        }
    }

}