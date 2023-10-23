using System.Management.Automation;
using PSSimpleConfig.Utilities;
using static PSSimpleConfig.Utilities.PSSC;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PSSimpleConfig.Cmdlets;

[Cmdlet(VerbsCommon.Set, "PSSConfigItem")]
public class SetPSSConfigItem : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0)]
    public string Path { get; set; }

    [Parameter(Mandatory = false, Position = 1)]
    public object? Value { get; set; }

    [Parameter(Mandatory = false)]
    public DirectoryInfo PathOverride { get; set; } = new DirectoryInfo(".");

    [Parameter(Mandatory = false)]
    public SwitchParameter PassThru { get; set; }
    protected override void ProcessRecord()
    {
        JObject output = new JObject();
        JObject jObject;
        try {
            jObject = PSSC.ImportConfig(PathOverride);
        }
        catch (Exception e) {
            throw new InvalidOperationException($"Unable to import configuration file for project: {PathOverride.Name}. Error: {e.Message}");
        }

        try {
            JToken jToken = jObject.SelectToken(Path);
            if (jToken != null)
            {
                if (Value is null) {
                    jToken.Parent.Remove();
                }
                else
                {
                    jToken.Replace(JToken.FromObject(Value));
                }
                output = jObject;
            }
            else
            {
                JObject currentObject = jObject;
                string[] keys = Path.Split('.');
                for (int i = 0; i < keys.Length; i++)
                {
                    string key = keys[i];

                    // If the key exists, move down the object tree
                    if (currentObject != null && currentObject.ContainsKey(key))
                    {
                        currentObject = (JObject)currentObject[key];
                    }
                    else
                    {
                        // Otherwise, create a new object or assign the value
                        if (i == keys.Length - 1)
                        {
                            // If this is the last key, set the value
                            WriteVerbose($"Adding '{key}' with value '{Value}'");
                            currentObject.Add(key, JToken.FromObject(Value));
                        }
                        else
                        {
                            // Otherwise, create a new JObject for the key
                            WriteVerbose($"Adding subkey '{key}'.");
                            JObject newObject = new JObject();
                            currentObject.Add(key, newObject);
                            currentObject = newObject;
                        }
                    }
                }
                WriteDebug($"Assigning 'currentObject' to 'output'");
                output = currentObject;
            }
            WriteVerbose($"Exporting configuration file to {PathOverride.FullName}");
            ExportConfig(PathOverride, output);
            if (PassThru)
            {
                WriteObject(JsonConvert.SerializeObject(output));
            }
        }
        catch (Exception e) {
            throw new InvalidOperationException($"Unable to update configuration '{Path}' with value '{Value}'. Error: {e.Message}");
        }
    }
}