using System.Management.Automation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PSSimpleConfig.Utilities;

namespace PSSimpleConfig.Cmdlets;

[Cmdlet(VerbsCommon.New, "PSSConfig", SupportsShouldProcess = false, ConfirmImpact = ConfirmImpact.None)]
public class NewPSSConfig : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 1)]
    [ValidateNotNullOrEmpty()]
    [Alias("Project")]
    public string? Name { get; set; }

    [Parameter(Mandatory = false, Position = 2)]
    [ValidateSet("User", "Machine")]
    public string Scope { get; set; } = "User";

    [Parameter(Mandatory = false)]
    public SwitchParameter Force { get; set; }

    [Parameter(Mandatory = false)]
    public System.IO.DirectoryInfo? Path { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter FromPSModule { get; set; }

    protected override void BeginProcessing()
    {
        PSSC.InitializeModule(this);
    }
    protected override void ProcessRecord()
    {
        WriteDebug($"PSSCCfgRoot: {PSSC.Instance.ModuleData["PSSCCfgRoot"]}");
        WriteDebug($"PSSCCfgFile: {PSSC.Instance.ModuleData["PSSCCfgFile"]}");
        WriteDebug($"ProjectRoot: {PSSC.Instance.ModuleData["ProjectRoot"]}");

        // Need to check if there's already a project with the same name.

        Guid projectGuid = Guid.NewGuid();

        bool v = Path == null;
        #pragma warning disable CS8618 // Nullability warning. We're checking nulls above.
        string projectPath = v ? System.IO.Path.Combine(PSSC.Instance.ModuleData["ProjectRoot"].ToString(), Name) : Path.FullName;
        #pragma warning restore CS8618 // Nullability warning.

        // Check if the project directory exists. If it does, check if we're forcing overwrite.
        if (System.IO.Directory.Exists(projectPath))
        {
            if (Force)
            {
                WriteWarning($"Project directory {projectPath} already exists. Forcing overwrite.");
                System.IO.Directory.Delete(projectPath, true);
                System.IO.Directory.CreateDirectory(projectPath);
            }
            else
            {
                WriteError(new ErrorRecord(new System.IO.IOException($"Project directory {projectPath} already exists. Use -Force to overwrite."), "ProjectExists", ErrorCategory.ResourceExists, projectPath));
                return;
            }
        }
    }
}
