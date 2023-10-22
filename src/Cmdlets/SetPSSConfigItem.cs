using System.IO;
using System.Collections;
using System.Management.Automation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PSSimpleConfig.Utilities;

namespace PSSimpleConfig.Cmdlets;

/// <summary>
/// <para type="synopsis">Set a value in a PSSimpleConfig project.</para>
///
/// <para type="description">The Set-PSSConfig cmdlet sets a value in a PSSimpleConfig project. When
/// a configuration path is provided, the cmdlet will attempt to traverse the given path to set
/// properties and values as given. The resulting JSON is written to the corresponding config.json
/// file in the project folder.</para>
///
/// <example>
///     <code>C:\> Set-PSSConfig -Name MyCoolProject -Path MyModule.Version -Value 1.0.0</code>
///     <para>Set the value of MyModule.Version to 1.0.0 in the MyCoolProject project config file.
///     this would result in the following JSON:</para>
///     <para/>
///     <code>
///     {
///         "MyModule": {
///             "Version": "1.0.0"
///         }
///     }
///     </code>
///     <para/>
/// </example>
///
/// <example>
///     <para>The next example will use this JSON as a starting point:</para>
///     <para/>
///     <code>
///     {
///         "PSSC": {
///             "Version": "1.0.0",
///             "ProjectName": "MyCoolProject",
///             "ConfigScope": "User"
///         }
///     }
///     </code>
///     <para/>
///     <code>C:\> Set-PSSConfig -Name MyCoolProject -Path PSSC -Value @{Version = "1.0.1"}</code>
///     <para>This will overwrite the PSSC node with the hashtable provided, resulting in the following JSON:</para>
///     <para/>
///     <code>
///     {
///         "PSSC": {
///             "Version": "1.0.1"
///        }
///     }
///     </code>
///     <para/>
///     <para>Notice that the ProjectName and ConfigScope properties were removed.</para>
/// </example>
///
/// <example>
///     <para>The next example will use this JSON as a starting point:</para>
///     <para/>
///     <code>
///     {
///         "PSSC": {
///             "Version": "1.0.0",
///             "ProjectName": "MyCoolProject",
///             "ConfigScope": "User"
///         }
///     }
///     </code>
///     <para/>
///     <code>C:\> Set-PSSConfig -Name MyCoolProject -Path PSSC.Version -Value 1.0.1</code>
///     <para/>
///     <code>
///     {
///         "PSSC": {
///             "Version": "1.0.1"
///             "ProjectName": "MyCoolProject",
///             "ConfigScope": "User"
///        }
///     }
///     </code>
///     <para/>
///     <para>Notice that the ProjectName and ConfigScope properties were _not_ removed.</para>
/// </example>
/// <example>
///     <para>Add nested keys based on path provided.</para>
///     <para/>
///
///     <code>C:\> Set-PSSConfig -Project MyCoolProject -Path New.key.for.the.win -Value "peanutbutter"
///     <para/>
///     <code>
///     {
///       "PSSC": {
///         "Version": "0.0.8693.20",
///         "ProjectName": "MyCoolProject",
///         "ConfigScope": "User"
///       },
///       "New": {
///         "key": {
///           "for": {
///             "the": {
///               "win": "peanutbutter"
///             }
///           }
///         }
///       }
///     }
///     </code>
///     <para/>
///     <para>The nested key structure was added to the JSON.</para>
/// </example>
/// <example>
///     <para>Add a sibling node to an existing path.</para>
///     <para/>
///     <code>C:\> Set-PSSConfig -Project MyCoolProject -Path PSSC.ValidSchema -Value $true</code>
///     <para/>
///     <code>
///     {
///         "PSSC": {
///             "Version": "0.0.8693.20",
///             "ProjectName": "PSIDM.Universal",
///             "ConfigScope": "User",
///             "ValidSchema": true
///         }
///     }
///     </code>
/// </example>
/// </summary>
[Cmdlet(VerbsCommon.Set, "PSSConfigItem")]
public class SetPSSConfigItem : PSCmdlet
{
    /// <summary>
    /// <para type="description">The name of the project to set the value in.</para>
    /// </summary>
    [Parameter(Mandatory = false)]
    [Alias("Project")]
    public string Name { get; set; } = "Default";

    /// <summary>
    /// <para type="description">The path to the value to set in dot notation (e.g; Path.To.Value).</para>
    /// </summary>
    [Parameter(Mandatory = false, Position = 0)]
    public string? Path { get; set; }

    /// <summary>
    /// <para type="description">The value to set.</para>
    /// </summary>
    [Parameter(Mandatory = true, Position = 1)]
    public object Value { get; set; }

    protected override void ProcessRecord()
    {
        string configFilePath = System.IO.Path.Combine(PSSimpleConfig.ProjectRoot, Name, "config.json");
        _ = new JObject();
        JObject jObject;

        try
        {
            jObject = PSSimpleConfig.ImportConfig(Name);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Unable to import configuration file for project: {Name}. Error: {e.Message}");
        }

        try {
            JToken jToken = jObject.SelectToken(Path);
            if (jToken != null)
            {
                jToken.Replace(JToken.FromObject(Value));
                PSSimpleConfig.ExportConfig(Name, jObject);
                WriteObject(JsonConversion.ToPSOutput(jObject));
            }
            else
            {
                // Initialize variables to hold current state
                JObject currentObject = jObject;
                string[] keys = Path.Split('.');
                for (int i = 0; i < keys.Length; i++)
                {
                    string key = keys[i];

                    // If the key exists, move down the object tree
                    if (currentObject != null && currentObject.ContainsKey(key))
                    {
                        currentObject = (JObject)currentObject[key];
                    }
                    else
                    {
                        // Otherwise, create a new object or assign the value
                        if (i == keys.Length - 1)
                        {
                            // If this is the last key, set the value
                            currentObject.Add(key, JToken.FromObject(Value));
                        }
                        else
                        {
                            // Otherwise, create a new JObject for the key
                            JObject newObject = new JObject();
                            currentObject.Add(key, newObject);
                            currentObject = newObject;
                        }
                    }
                }
                PSSimpleConfig.ExportConfig(Name, currentObject);
                WriteObject(JsonConversion.ToPSOutput(currentObject));
            }
        }
        catch (Exception e) {
            throw new InvalidOperationException($"Unable to update configuration for project: {Name}. Error: {e.Message}");
        }
    }
}