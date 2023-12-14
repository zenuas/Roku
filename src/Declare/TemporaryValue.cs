using Roku.Manager;

namespace Roku.Declare;

public class TemporaryValue(string name, int index, ILexicalScope scope) : IEvaluable
{
    public string Name { get; } = name;
    public int Index { get; } = index;
    public ILexicalScope Scope { get; } = scope;

    public override string ToString() => Name;
}
