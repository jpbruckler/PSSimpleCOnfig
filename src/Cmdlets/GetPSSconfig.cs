using System.Management.Automation;
using Newtonsoft.Json.Linq;

using PSSimpleConfig.Utilities;

namespace PSSimpleConfig.Cmdlets;

[Cmdlet(VerbsCommon.Get, "PSSConfig", DefaultParameterSetName = "List")]
public class GetPSSConfig : PSCmdlet
{
    [Parameter(Mandatory = false, Position = 0, ParameterSetName = "Get")]
    public string? Path { get; set; }

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
        PSSimpleConfig.SetScope(Scope);
    }
    protected override void ProcessRecord()
    {
        if (this.ParameterSetName == "List")
        {
            string[] dirs = Directory.GetDirectories(PSSimpleConfig.ProjectRoot);
            foreach (string dir in dirs)
            {
                PSObject psObject = new PSObject();
                psObject.Properties.Add(new PSNoteProperty("Scope", PSSimpleConfig.Scope));
                psObject.Properties.Add(new PSNoteProperty("ProjectName", dir.Replace(PSSimpleConfig.ProjectRoot, "").TrimStart('\\')));
                psObject.Properties.Add(new PSNoteProperty("ConfigPath", System.IO.Path.Combine(dir, "config.json")));
                WriteObject(psObject);
            }
        }
        else
        {
            string configFilePath = System.IO.Path.Combine(PSSimpleConfig.ProjectRoot, Name, "config.json");
            if (File.Exists(configFilePath))
            {
                string jsonContent = File.ReadAllText(configFilePath);
                // Parse the JSON content into a JObject
                JObject jObject = JObject.Parse(jsonContent);

                if (Path == null)
                {
                    var _output = JsonConversion.ToPSObject(jObject);
                    WriteObject(_output);
                }
                else {
                    // Use LINQ to select the desired value
                    JToken jToken = jObject.SelectToken(Path);
                    object result = JsonConversion.ToPSOutput(jToken);
                    WriteObject(result);
                }

            }
            else
            {
                WriteError(new ErrorRecord(new FileNotFoundException($"Could not find config file at {configFilePath}"), "ConfigFileNotFound", ErrorCategory.ObjectNotFound, null));
            }
        }
    }
}