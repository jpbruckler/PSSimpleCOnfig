using System.Management.Automation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace PSSimpleConfig.Utilities;
public sealed class PSSC
{
    private static readonly object Lock = new object();
    private static PSSC? s_instance = null;
    public Dictionary<Guid, Project> Projects { get; private set; } = new Dictionary<Guid, Project>();
    public Dictionary<string, object> ModuleData { get; private set; } = new Dictionary<string, object>();
    private PSSC()
    {
        // Initialize the Projects Dictionary
        //Projects = new Dictionary<Guid, Project>();
        //ModuleData = new Dictionary<string, object>();
    }

    public static PSSC Instance
    {
        get
        {
            lock (Lock)
            {
                s_instance ??= new PSSC();
                return s_instance;
            }
        }
    }

    public Project GetProjectByName(string name)
    {
        return Projects.Values.FirstOrDefault(p => p.Name == name);
    }

    public static void InitializeModule(PSCmdlet instance)
    {
        /// <summary>
        /// The root path for the PSSimpleConfig module conifguration file. Defaults
        /// to the directory containing the module, but if that can't be determined
        /// it will default to the CommonApplicationData ($env:ProgramData) folder.
        /// </summary>
        string _moduleCfgRoot = string.Empty;

        /// <summary>
        /// The full path to the PSSimpleConfig module configuration file.
        /// </summary>
        string _moduleCfgFile = string.Empty;

        /// <summary>
        /// The root path for the PSSimpleConfig project configuration files. Defaults
        /// to the CommonApplicationData ($env:ProgramData) folder.
        /// </summary>
        string _projectCfgRoot = string.Empty;

        /// <summary>
        /// 1. Check that the default config path exists. Create if it doesn't.
        /// 2. Check that the default config file exists. Create if it doesn't.
        /// 3. Check for project name collision, throw if there is a name collision.
        /// </summary>
        if (string.IsNullOrEmpty(_moduleCfgRoot)) {
            _moduleCfgRoot = Path.GetDirectoryName(instance.MyInvocation?.MyCommand?.Module?.Path)
                                ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "PSSimpleConfig");
        }
        if (string.IsNullOrEmpty(_moduleCfgFile)) {
            _moduleCfgFile = Path.Combine(_moduleCfgRoot, "PSSimpleConfig.json");
        }
        if (string.IsNullOrEmpty(_projectCfgRoot)) {
            _projectCfgRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "PSSimpleConfig", "Projects");
        }

        if (Directory.Exists(_projectCfgRoot)) {
            Directory.CreateDirectory(_projectCfgRoot);
        }

        if (!File.Exists(_moduleCfgFile)) {
            var moduleCfg = new JObject
            {
                { "PSSCCfgRoot", _moduleCfgRoot },
                { "PSSCfgFile", _moduleCfgFile },
                { "ProjectRoot", _projectCfgRoot }
            };
            File.WriteAllText(_moduleCfgFile, moduleCfg.ToString(Formatting.Indented));
        }

        if (Instance.ModuleData.Count > 0) {
            Instance.ModuleData.Clear();
        }
        var moduleData = JObject.Parse(File.ReadAllText(_moduleCfgFile));
        foreach (var kvp in moduleData) {
            Instance.ModuleData.Add(kvp.Key, kvp.Value == null ? string.Empty : kvp.Value.ToString());
        }
    }
}