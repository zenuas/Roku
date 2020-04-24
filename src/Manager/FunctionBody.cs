using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class FunctionBody : IFunctionBody, ILexicalScope
    {
        public string Name { get; }
        public VariableValue? Return { get; set; } = null;
        public List<(VariableValue Name, VariableValue Type)> Arguments { get; } = new List<(VariableValue, VariableValue)>();
        public List<Operand> Body { get; } = new List<Operand>();
        public INamespace Namespace { get; }
        public ILexicalScope? Parent { get; } = null;
        public Dictionary<string, ITypedValue> LexicalScope { get; } = new Dictionary<string, ITypedValue>();
        public Dictionary<ITypedValue, IStructBody?> TypeMapper { get; } = new Dictionary<ITypedValue, IStructBody?>();

        public FunctionBody(INamespace ns, string name)
        {
            Namespace = ns;
            Name = name;
        }
    }
}
