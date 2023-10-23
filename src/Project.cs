namespace PSSimpleConfig;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Root { get; set; }

    public Project(string name, string root)
    {
        Id = Guid.NewGuid();
        Name = name;
        Root = root;
    }

    public Project(Guid id, string name, string root)
    {
        Id = id;
        Name = name;
        Root = root;
    }

    public override string ToString()
    {
        return $"{Name} ({Id})";
    }

}