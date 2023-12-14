using Roku.Declare;
using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager;

public class InnerScope(ILexicalScope parent) : ILexicalScope
{
    public List<IOperand> Body { get; } = [];
    public IManaged Namespace { get; } = parent.Namespace;
    public ILexicalScope? Parent { get; } = parent;
    public Dictionary<string, IEvaluable> LexicalScope { get; } = [];
    public int MaxTemporaryValue { get; set; } = parent.MaxTemporaryValue;
}
