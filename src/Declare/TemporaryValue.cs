using Roku.Manager;

namespace Roku.Declare;

public class TemporaryValue : IEvaluable
{
    public required string Name { get; init; }
    public required int Index { get; init; }
    public required ILexicalScope Scope { get; init; }

    public override string ToString() => Name;
}
