using System.IO;
using System.Management.Automation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PSSimpleConfig.Utilities;

public static class PSSC
{
    public static FileInfo Initialize(DirectoryInfo directory)
    {
        string _configPath = Path.Combine(directory.FullName, "config", "config.json");

        if (!Directory.Exists(Path.Combine(directory.FullName, "config")))
        {
            try {
                Directory.CreateDirectory(Path.Combine(directory.FullName, "config"));
            }
            catch (Exception e) {
                throw new IOException($"Unable to create config directory for project: {directory.Name}. Error: {e.Message}");
            }
        }

        if (!File.Exists(_configPath))
        {
            try {
                File.WriteAllText(_configPath, "{}");
            }
            catch (Exception e) {
                throw new IOException($"Unable to create config file for project: {directory.Name}. Error: {e.Message}");
            }
        }
        return new FileInfo(_configPath);
    }

    public static JObject ImportConfig(DirectoryInfo directory)
    {
        FileInfo _configPath = Initialize(directory);
        try {
            if (!File.Exists(_configPath.FullName))
            {
                throw new FileNotFoundException($"Configuration file not found at path: {_configPath.FullName}");
            }
            return JObject.Parse(File.ReadAllText(_configPath.FullName));
        }
        catch (Exception e) {
            throw new InvalidOperationException($"Unable to import configuration file for project: {directory.Name}. Error: {e.Message}");
        }
    }

    public static void ExportConfig(DirectoryInfo directory, JObject jObject)
    {
        FileInfo _configPath = Initialize(directory);
        try {
            File.WriteAllText(_configPath.FullName, jObject.ToString());
        }
        catch (Exception e) {
            throw new InvalidOperationException($"Unable to export configuration file for project: {directory.Name}. Error: {e.Message}");
        }
    }
}