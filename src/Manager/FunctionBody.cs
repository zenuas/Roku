using Roku.IntermediateCode;
using Roku.TypeSystem;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class FunctionBody : IFunctionBody, ILexicalScope
    {
        public IFunction Function { get; set; }
        public VariableValue? Return { get; set; } = null;
        public List<(VariableValue Name, VariableValue Type)> Arguments { get; } = new List<(VariableValue, VariableValue)>();
        public List<Operand> Body { get; } = new List<Operand>();
        public INamespace Namespace { get; }
        public ILexicalScope? Parent { get; } = null;
        public Dictionary<string, IType?> LexicalScope { get; } = new Dictionary<string, IType?>();

        public FunctionBody(INamespace ns, IFunction f)
        {
            Namespace = ns;
            Function = f;
        }
    }
}
