using System.Management.Automation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PSSimpleConfig.Utilities;

namespace PSSimpleConfig.Cmdlets;

[Cmdlet(VerbsData.Import, "PSSConfig", SupportsShouldProcess = false, ConfirmImpact = ConfirmImpact.None)]
public class ImportPSSConfig
{

}