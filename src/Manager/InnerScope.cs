using Roku.Declare;
using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class InnerScope : ILexicalScope
    {
        public List<IOperand> Body { get; } = new List<IOperand>();
        public INamespace Namespace { get; }
        public ILexicalScope? Parent { get; } = null;
        public Dictionary<string, IEvaluable> LexicalScope { get; } = new Dictionary<string, IEvaluable>();
        public int MaxTemporaryValue { get; set; } = 0;

        public InnerScope(ILexicalScope parent)
        {
            Namespace = parent.Namespace;
            Parent = parent;
            MaxTemporaryValue = parent.MaxTemporaryValue;
        }
    }
}
