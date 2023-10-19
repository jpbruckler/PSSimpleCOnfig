using System;
using System.IO;
using System.Management.Automation;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace PSSimpleConfig;

[Cmdlet(VerbsCommon.New, "PSSConfig")]
public class RegisterPSSConfig : PSCmdlet
{
    [Parameter(Mandatory = false, Position = 1)]
    [Alias("Project")]
    public string? Name { get; set; }

    [Parameter(Mandatory = false, Position = 2)]
    [ValidateSet("User", "Machine")]
    public string Scope { get; set; } = "User";

    [Parameter(Mandatory = false)]
    public SwitchParameter Force { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter FromPSModule { get; set; }

    protected override void ProcessRecord()
    {
        ConfigRoot.SetScope(Scope);
        WriteDebug($"ConfigRoot.Scope: {ConfigRoot.Scope}");
        WriteDebug($"ConfigRoot.Root: {ConfigRoot.Root}");
        WriteDebug($"ConfigRoot.Namespaces: {ConfigRoot.Namespaces}");

        try
        {
            // Create the root (PSSimpleConfig) directory if it doesn't exist
            if (!Directory.Exists(ConfigRoot.Root))
            {
                WriteVerbose($"Creating PSSimpleConfig Root directory: {ConfigRoot.Root}");
                Directory.CreateDirectory(ConfigRoot.Root);
            }
            // Create the namespaces directory if it doesn't exist
            // root/namespaces
            if (!Directory.Exists(ConfigRoot.Namespaces))
            {
                WriteVerbose($"Creating namespaces directory: {ConfigRoot.Namespaces}");
                Directory.CreateDirectory(ConfigRoot.Namespaces);
            }
            // Create the project directory if it doesn't exist
            if (!string.IsNullOrEmpty(Name))
            {
                string projectFolder = Path.Combine(ConfigRoot.Namespaces, Name);   // root/namespaces/project
                bool _createProject = false;                                        // Flag to determine if we need to create the project directory
                if (!Directory.Exists(projectFolder))
                {
                    _createProject = true;
                }
                else
                {
                    if (Force == false)
                    {
                        WriteWarning($"Project {Name} already exists. Use -Force to overwrite.");
                    }
                    else
                    {
                        WriteVerbose($"Deleting existing project directory: {projectFolder}");
                        Directory.Delete(projectFolder, true);
                        _createProject = true;
                    }
                }
                if (_createProject)
                {
                    WriteVerbose($"Creating project directory: {projectFolder}");
                    Directory.CreateDirectory(projectFolder);

                    // Create a sample config.json file.
                    // if (FromPSModule) is true, then we'll create a sample
                    // config.json file from the module named in the Name parameter.
                    // Otherwise, we'll create a sample config.json file scratch.
                    PSObject configObject = new PSObject();

                    configObject.Properties.Add(new PSNoteProperty("Name", Name));
                    configObject.Properties.Add(new PSNoteProperty("ConfigScope", Scope));

                    // Use Get-Module to retrive some basic information about
                    // the module and add it to the configObject.
                    if (FromPSModule)
                    {
                        using PowerShell ps = PowerShell.Create();
                        ps.AddCommand("Get-Module").AddParameter("Name", Name).AddParameter("ListAvailable");
                        Collection<PSObject> module = ps.Invoke();

                        configObject.Properties.Add(new PSNoteProperty("Version", module[0].Properties["Version"].Value.ToString()));
                        configObject.Properties.Add(new PSNoteProperty("ModuleRoot", module[0].Properties["ModuleBase"].Value));
                    }

                    // Convert PSObject to Dictionary
                    Dictionary<string, object> dictionary = new Dictionary<string, object>();
                    foreach (var property in configObject.Properties)
                    {
                        dictionary.Add(property.Name, property.Value);
                    }

                    // Serialize Dictionary to JSON
                    string configJson = JsonConvert.SerializeObject(dictionary, Formatting.Indented);

                    File.WriteAllText(Path.Combine(projectFolder, "config.json"), configJson.ToString());
                    WriteObject(configObject);
                }
            }
        }
        catch (Exception e)
        {
            ErrorRecord errorRecord = new(e, $"Could not create directory structure for {Name}.", ErrorCategory.InvalidOperation, null);
            ThrowTerminatingError(errorRecord);
        }
    }
}
