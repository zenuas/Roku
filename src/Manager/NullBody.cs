namespace Roku.Manager;

public class NullBody : IStructBody
{
    public string Name => "Null";

    public override string ToString() => Name;
}
