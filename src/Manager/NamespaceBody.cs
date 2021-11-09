namespace Roku.Manager;

public class NamespaceBody : IStructBody
{
    public string Name { get; init; } = "";
    public NamespaceBody? Parent { get; set; }
}
