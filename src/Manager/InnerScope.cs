using Roku.Declare;
using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager;

public class InnerScope(ILexicalScope parent) : ILexicalScope
{
    public List<IOperand> Body { get; init; } = [];
    public IManaged Namespace { get; init; } = parent.Namespace;
    public ILexicalScope? Parent { get; init; } = parent;
    public Dictionary<string, IEvaluable> LexicalScope { get; init; } = [];
    public int MaxTemporaryValue { get; set; } = parent.MaxTemporaryValue;
}
