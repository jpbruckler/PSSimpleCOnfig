using System.Management.Automation;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace PSSimpleConfig.Cmdlets;

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
        PSSimpleConfig.SetScope(Scope);
        WriteDebug($"PSSimpleConfig.Scope: {PSSimpleConfig.Scope}");
        WriteDebug($"PSSimpleConfig.Root: {PSSimpleConfig.Root}");
        WriteDebug($"PSSimpleConfig.Namespaces: {PSSimpleConfig.Namespaces}");

        try
        {
            // Create the root (PSSimpleConfig) directory if it doesn't exist
            if (!Directory.Exists(PSSimpleConfig.Root))
            {
                WriteVerbose($"Creating PSSimpleConfig Root directory: {PSSimpleConfig.Root}");
                Directory.CreateDirectory(PSSimpleConfig.Root);
            }
            // Create the namespaces directory if it doesn't exist
            // root/namespaces
            if (!Directory.Exists(PSSimpleConfig.Namespaces))
            {
                WriteVerbose($"Creating namespaces directory: {PSSimpleConfig.Namespaces}");
                Directory.CreateDirectory(PSSimpleConfig.Namespaces);
            }
            // Create the project directory if it doesn't exist
            if (!string.IsNullOrEmpty(Name))
            {
                string projectFolder = Path.Combine(PSSimpleConfig.Namespaces, Name);   // root/namespaces/project
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
