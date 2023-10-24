using System.Management.Automation;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PSSimpleConfig.Utilities;

public class PSSC
{
    private static readonly Lazy<PSSC> _instance = new Lazy<PSSC>(() => new PSSC());

    private FileInfo _configPath;
    public FileInfo ConfigPath
    {
        get { return _configPath; }
        private set { _configPath = value; }
    }

    private PSSC()
    {
        // Initialize with some default value if needed
        _configPath = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "config", "config.json"));
    }

    public static PSSC Instance => _instance.Value;

    public void Initialize(DirectoryInfo? directory)
    {
        if (directory is null)
        {
            directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        }
        Initialize(new FileInfo(Path.Combine(directory.FullName, "config", "config.json")));
    }

    public void Initialize(FileInfo fileInfo)
    {
        if (fileInfo is null)
        {
            throw new ArgumentNullException(nameof(fileInfo));
        }

        string pathToCfgFile = fileInfo.FullName;
        DirectoryInfo directory = new DirectoryInfo(fileInfo.DirectoryName);

        if (!Directory.Exists(directory.FullName))
        {
            try
            {
                Directory.CreateDirectory(directory.FullName);
            }
            catch (Exception e)
            {
                throw new IOException($"Unable to create config directory for project: {directory.Name}. Error: {e.Message}");
            }
        }

        if (!File.Exists(pathToCfgFile))
        {
            try
            {
                File.WriteAllText(pathToCfgFile, "{}");
            }
            catch (Exception e)
            {
                throw new IOException($"Unable to create config file for project: {pathToCfgFile}. Error: {e.Message}");
            }
        }
        _configPath = new FileInfo(pathToCfgFile);
    }

    public void UpdateConfigPath(FileInfo filePath)
    {
        if (filePath is null)
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        if (!File.Exists(filePath.FullName))
        {
            throw new FileNotFoundException($"Configuration file not found at path: {filePath.FullName}");
        }
        _configPath = filePath;
    }

    public JObject ImportConfig(FileInfo configFilePath)
    {
        // If the path is null, use the default path
        configFilePath ??= _configPath;
        try
        {
            return !File.Exists(configFilePath.FullName)
                ? throw new FileNotFoundException($"Configuration file not found at path: {configFilePath.FullName}")
                : JObject.Parse(File.ReadAllText(configFilePath.FullName));
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Unable to import configuration file for project: {configFilePath.Name}. Error: {e.Message}");
        }
    }

    public void ExportConfig(FileInfo configFilePath, JObject jObject)
    {
        // If the path is null, use the default path
        configFilePath ??= _configPath;

        try
        {
            File.WriteAllText(configFilePath.FullName, jObject.ToString());
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Unable to export configuration file for project: {configFilePath.Name}. Error: {e.Message}");
        }
    }

    public static DirectoryInfo GetModuleBasePath(string moduleName)
    {
        PowerShell ps = PowerShell.Create();

        ps.AddCommand("Get-Module").AddParameter("Name", moduleName).AddParameter("ListAvailable");
        var result = ps.Invoke();
        if (result is null)
        {
            throw new InvalidOperationException($"Unable to find module: {moduleName}");
        }
        else
        {
            //return new FileInfo(result[0].Members["Path"].Value.ToString());
            var moduleBaseProperty = result[0].Properties?["ModuleBase"];
            return moduleBaseProperty is null
                ? throw new InvalidOperationException($"Unable to find module: {moduleName}")
                : new DirectoryInfo(moduleBaseProperty.Value.ToString());
        }
    }
}