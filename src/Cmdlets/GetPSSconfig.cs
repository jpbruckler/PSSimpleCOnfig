using System;
using System.IO;
using System.Management.Automation;

using Newtonsoft.Json.Linq;

namespace PSSimpleConfig;

[Cmdlet(VerbsCommon.Get, "PSSConfig", DefaultParameterSetName = "List")]
public class GetPSSConfig : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Get")]
    [Alias("Path")]
    public string ConfigPath { get; set; }

    [Parameter(Mandatory = false, Position = 1, ParameterSetName = "Get")]
    [Alias("Project")]
    public string Name { get; set; } = "DefaultNamespace";

    [Parameter(Mandatory = false, ParameterSetName = "List")]
    public SwitchParameter ListAvailable { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("User", "Machine")]
    public string Scope { get; set; } = "User";

    protected override void BeginProcessing()
    {
        base.BeginProcessing();
        ConfigRoot.SetScope(Scope);
    }
    protected override void ProcessRecord()
    {
        if (this.ParameterSetName == "List")
        {
            string[] dirs = Directory.GetDirectories(ConfigRoot.Namespaces);
            foreach (string dir in dirs)
            {
                PSObject psObject = new PSObject();
                psObject.Properties.Add(new PSNoteProperty("Scope", ConfigRoot.Scope));
                psObject.Properties.Add(new PSNoteProperty("ProjectName", dir.Replace(ConfigRoot.Namespaces, "").TrimStart('\\')));
                psObject.Properties.Add(new PSNoteProperty("ConfigPath", Path.Combine(dir, "config.json")));
                WriteObject(psObject);
            }
        }
        else
        {
            string configFilePath = Path.Combine(ConfigRoot.Namespaces, Name, "config.json");
            if (File.Exists(configFilePath))
            {
                string jsonContent = File.ReadAllText(configFilePath);
                // Parse the JSON content into a JObject
                JObject jObject = JObject.Parse(jsonContent);

                // Use LINQ to select the desired value
                JToken jToken = jObject.SelectToken(ConfigPath);
                WriteObject(jToken?.ToObject<object>());
            }
            else
            {
                WriteError(new ErrorRecord(new FileNotFoundException($"Could not find config file at {configFilePath}"), "ConfigFileNotFound", ErrorCategory.ObjectNotFound, null));
            }
        }
    }
}