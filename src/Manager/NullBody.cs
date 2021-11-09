namespace Roku.Manager;

public class NullBody : IStructBody
{
    public string Name { get => "Null"; }

    public override string ToString() => Name;
}
