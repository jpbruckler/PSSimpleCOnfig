using Newtonsoft.Json.Linq;
using System.Collections.Generic;


namespace PSSimpleConfig;
public sealed class PSSimpleConfigSingleton
{
    private static readonly object _lock = new object();
    private static PSSimpleConfigSingleton _instance = null;

    public Dictionary<string, JObject> ScopedData { get; private set; }

    private PSSimpleConfigSingleton()
    {
        // Initialize the ScopedData Dictionary
        ScopedData = new Dictionary<string, JObject>();
    }

    public static PSSimpleConfigSingleton Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new PSSimpleConfigSingleton();
                }
                return _instance;
            }
        }
    }

    public void UpdateScopedData(string guid, JObject projectData)
    {
        // Here you can add validation or additional logic
        ScopedData[guid] = projectData;
    }

    public JObject GetScopedData(string guid)
    {
        if (ScopedData.TryGetValue(guid, out var projectData))
        {
            return projectData;
        }
        return null;
    }
}
