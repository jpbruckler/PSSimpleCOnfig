using System;
using System.IO;
using System.Diagnostics;
using System.Management.Automation;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace PSSimpleConfig.Cmdlets;

[Cmdlet(VerbsCommon.New, "PSSConfig", SupportsShouldProcess = false, ConfirmImpact = ConfirmImpact.None)]
public class NewPSSConfig : PSCmdlet
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
        WriteDebug($"PSSimpleConfig.ProjectRoot: {PSSimpleConfig.ProjectRoot}");


        PSVariableIntrinsics _sessionState = SessionState.PSVariable;
        // Create a PowerShell instance for retrieving module information
        using PowerShell ps = PowerShell.Create();

        try
        {
            // Create the root (PSSimpleConfig) directory if it doesn't exist
            if (!Directory.Exists(PSSimpleConfig.Root))
            {
                WriteDebug($"PSSimpleConfig root directory not found. Creating directory: {PSSimpleConfig.Root}");
                WriteVerbose($"Creating PSSimpleConfig Root directory: {PSSimpleConfig.Root}");
                Directory.CreateDirectory(PSSimpleConfig.Root);
            }
            // Create the projects directory if it doesn't exist
            // root/projects
            if (!Directory.Exists(PSSimpleConfig.ProjectRoot))
            {
                WriteDebug($"PSSimpleConfig projects directory not found. Creating directory: {PSSimpleConfig.ProjectRoot}");
                WriteVerbose($"Creating project directory: {PSSimpleConfig.ProjectRoot}");
                Directory.CreateDirectory(PSSimpleConfig.ProjectRoot);
            }
            // Create the project directory if it doesn't exist
            // root/projects/{Name}
            if (!string.IsNullOrEmpty(Name))
            {
                string projectFolder = Path.Combine(PSSimpleConfig.ProjectRoot, Name);
                bool _createProject = false;
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
                        WriteDebug($"Project {Name} exists and -Force is set. Deleting existing project directory: {projectFolder}");
                        WriteVerbose($"Deleting existing project directory: {projectFolder}");
                        Directory.Delete(projectFolder, true);
                        _createProject = true;
                    }
                }
                if (_createProject)
                {
                    WriteVerbose($"Creating project directory: {projectFolder}");
                    Directory.CreateDirectory(projectFolder);


                    PSModuleInfo _PSSCMod = this.MyInvocation.MyCommand.Module;

                    // Create a Dictionary to hold the initial config.json values.
                    Dictionary<string, object> _configDict = new Dictionary<string, object>();
                    Dictionary<string, object> _psscInfo = new Dictionary<string, object>();

                    // Add the PSSimpleConfig module information to the _psscInfo Dictionary
                    _psscInfo.Add("Version", _PSSCMod.Version.ToString());
                    _psscInfo.Add("ProjectName", Name);
                    _psscInfo.Add("ConfigScope", Scope);
                    _configDict.Add("PSSC", _psscInfo);

                    // Use Get-Module to retrive some basic information about
                    // the module and add it to the configObject.
                    if (FromPSModule)
                    {
                        ps.AddCommand("Get-Module").AddParameter("Name", Name).AddParameter("ListAvailable");
                        Collection<PSObject> module = ps.Invoke();

                        _configDict.Add("Version", module[0].Properties["Version"].Value.ToString());
                        _configDict.Add("ModuleRoot", module[0].Properties["ModuleBase"].Value);
                    }

                    // Serialize Dictionary to JSON
                    string configJson = JsonConvert.SerializeObject(_configDict, Formatting.Indented);

                    File.WriteAllText(Path.Combine(projectFolder, "config.json"), configJson.ToString());
                    //WriteObject(_configDict);

                    _sessionState.Set("script:PSSimpleConfig", _configDict);
                    object output = _sessionState.Get("script:PSSimpleConfig");
                    WriteObject(output);
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
