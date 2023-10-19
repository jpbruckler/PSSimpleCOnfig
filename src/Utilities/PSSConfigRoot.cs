
namespace PSSimpleConfig;

public static class PSSimpleConfig
{
    public static string Scope { get; private set; } = "User";
    public static string Root { get; private set; }

    public static string Namespaces { get { return Path.Combine(Root, "namespaces"); }}
    static PSSimpleConfig()
    {
        Root = "";
        UpdateRoot();
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
        string rootFolder = Environment.GetFolderPath(
            Scope == "User" ? Environment.SpecialFolder.LocalApplicationData : Environment.SpecialFolder.CommonApplicationData
        );
        Root = Path.Combine(rootFolder, "PSSimpleConfig");
    }
}

