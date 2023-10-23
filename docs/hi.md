/// <summary>
/// <para type="synopsis">Set a value in a PSSimpleConfig project.</para>
///
/// <para type="description">The Set-PSSConfigItem cmdlet sets a value in a PSSimpleConfig project. When
/// a configuration path is provided, the cmdlet will attempt to traverse the given path to set
/// properties and values as given. The resulting JSON is written to the corresponding config.json
/// file in the project folder.</para>
///
/// <example>
///     <code>C:\> Set-PSSConfigItem -Path MyModule.Version -Value 1.0.0</code>
///     <para>Set the value of MyModule.Version to 1.0.0 in the config file.
///     This would result in the following JSON:</para>
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
///     <code>C:\> Set-PSSConfigItem -Path PSSC -Value @{Version = "1.0.1"}</code>
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
///     <code>C:\> Set-PSSConfigItem -Path PSSC.Version -Value 1.0.1</code>
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
///     <code>C:\> Set-PSSConfigItem -Path New.key.for.the.win -Value "peanutbutter"
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
///     <code>C:\> Set-PSSConfigItem -Path PSSC.ValidSchema -Value $true</code>
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
