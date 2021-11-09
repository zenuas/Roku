namespace Roku.Manager;

public class NamespaceBody : IStructBody
{
    public string Name { get; set; }
    public NamespaceBody? Parent { get; set; }

    public NamespaceBody(string name)
    {
        Name = name;
    }
}
