using System.Management.Automation;

namespace PSSimpleConfig;

[Cmdlet(VerbsLifecycle.Unregister, "PSSConfig", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
public class UnregisterPSSConfig : PSCmdlet
{
    [Parameter(Mandatory = false, Position = 0)]
    [ValidateSet("User", "Machine")]
    public string Scope { get; set; } = "User";

    [Parameter(Mandatory = true, Position = 1)]
    [ValidateNotNullOrEmpty()]
    [Alias("Project")]
    public string Name { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Force { get; set; }

    protected override void ProcessRecord()
    {
        ConfigRoot.SetScope(Scope);
        WriteDebug($"ConfigRoot.Scope: {ConfigRoot.Scope}");
        WriteDebug($"ConfigRoot.Root: {ConfigRoot.Root}");
        WriteDebug($"ConfigRoot.Namespaces: {ConfigRoot.Namespaces}");


        try
        {
            if (Force || ShouldProcess($"Unregister-PSSConfig -Scope {Scope} -Name {Name}"))
            {
                string projectFolder = Path.Combine(ConfigRoot.Namespaces, Name);
                if (Directory.Exists(projectFolder))
                {
                    WriteVerbose($"Deleting project directory: {projectFolder}");
                    Directory.Delete(projectFolder, true);
                }
                else
                {
                    WriteWarning($"Project {Name} does not exist.");
                }
            }
        }
        catch (Exception e)
        {
            ErrorRecord errorRecord = new ErrorRecord(e, $"Could not delete directory structure for {Name}.", ErrorCategory.InvalidOperation, null);
            ThrowTerminatingError(errorRecord);
        }
    }
}