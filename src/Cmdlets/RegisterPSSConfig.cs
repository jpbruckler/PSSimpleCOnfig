using System;
using System.IO;
using System.Management.Automation;

namespace PSSimpleConfig;

[Cmdlet(VerbsLifecycle.Register, "PSSConfig")]
public class RegisterPSSConfig : PSCmdlet
{
    [Parameter(Mandatory = false, Position = 1)]
    [Alias("Project")]
    public string? Name { get; set; }

    [Parameter(Mandatory = false, Position = 2)]
    [ValidateSet("User", "Machine")]
    public string Scope { get; set; } = "User";

    protected override void ProcessRecord()
    {
        ConfigRoot.SetScope(Scope);
        WriteDebug($"ConfigRoot.Scope: {ConfigRoot.Scope}");
        WriteDebug($"ConfigRoot.Root: {ConfigRoot.Root}");
        WriteDebug($"ConfigRoot.Namespaces: {ConfigRoot.Namespaces}");
        
        try {
            if (!Directory.Exists(ConfigRoot.Root))
            {
                WriteVerbose($"Creating PSSimpleConfig Root directory: {ConfigRoot.Root}");
                Directory.CreateDirectory(ConfigRoot.Root);
            }

            if (!Directory.Exists(ConfigRoot.Namespaces))
            {
                WriteVerbose($"Creating namespaces directory: {ConfigRoot.Namespaces}");
                Directory.CreateDirectory(ConfigRoot.Namespaces);
            }

            if (!string.IsNullOrEmpty(Name))
            {
                string projectFolder = Path.Combine(ConfigRoot.Namespaces, Name);
                if (!Directory.Exists(projectFolder))
                {
                    WriteVerbose($"Creating project directory: {projectFolder}");
                    Directory.CreateDirectory(projectFolder);
                    WriteObject(new DirectoryInfo(projectFolder));
                }
                else {
                    WriteWarning($"Project {Name} already exists.");
                }
            }
        }
        catch (Exception e) {
            ErrorRecord errorRecord = new ErrorRecord(e, $"Could not create directory structure for {Name}.", ErrorCategory.InvalidOperation, null);
            ThrowTerminatingError(errorRecord);
        }
    }
}
