using System.IO;
using System.Management.Automation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PSSimpleConfig;

public static class PSSimpleConfig
{
    public static string Scope { get; private set; } = "User";
    public static string Root { get; private set; }

    public static string ProjectRoot { get { return Path.Combine(Root, "projects"); }}
    static PSSimpleConfig()
    {
        UpdateRoot();

        if (Root == null) {
            Root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }
    }
    public static void SetScope(string configScope)
    {
        if (configScope == "User" || configScope == "Machine")
        {
            Scope = configScope;
            UpdateRoot();
        }
        else
        {
            throw new ArgumentException("Scope must be either 'User' or 'Machine'.");
        }
    }
    private static void UpdateRoot()
    {
        // Allow a user to override the default root folder with environment variable
        if (Environment.GetEnvironmentVariable("PSSC_ROOT") != null)
        {
            Root = Environment.GetEnvironmentVariable("PSSC_ROOT");
            return;
        }
        else if (Environment.GetEnvironmentVariable("PSSC_ROOT") == null)
        {
            string rootFolder = Environment.GetFolderPath(
                Scope == "User" ? Environment.SpecialFolder.LocalApplicationData : Environment.SpecialFolder.CommonApplicationData
            );
            Root = Path.Combine(rootFolder, "PSSimpleConfig");
        }
    }

    public static FileInfo GetConfigFilePath(string projectName)
    {
        return new FileInfo(Path.Combine(ProjectRoot, projectName, "config.json"));
    }

    public static JObject ImportConfig(string projectName)
    {
        FileInfo configFile = GetConfigFilePath(projectName);
        if (!configFile.Exists)
        {
            throw new FileNotFoundException($"Could not find config file for project {projectName}. Expected file path: {configFile.FullName}");
        }

        try {
            return JObject.Parse(File.ReadAllText(configFile.FullName));
        }
        catch (IOException e) {
            throw new IOException($"Could not read config file for project {projectName}. Expected file path: {configFile.FullName}. Error: {e.Message}");
        }
        catch (Exception e) {
            throw new Exception($"Could not read config file for project {projectName}. Error: {e.Message}");
        }
    }

    public static void ExportConfig(string projectName, JObject config)
    {
        FileInfo configFile = GetConfigFilePath(projectName);

        try {
            File.WriteAllText(configFile.FullName, config.ToString(Formatting.Indented));
        }
        catch {
            throw new IOException($"Could not write to config file for project {projectName}. Expected file path: {configFile.FullName}");
        }
    }
}

