using System;
using System.IO;
using System.Management.Automation;
using Newtonsoft.Json;

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
                WriteObject(dir.Replace(ConfigRoot.Namespaces, "").TrimStart('\\'));
            }
        }
    }
}