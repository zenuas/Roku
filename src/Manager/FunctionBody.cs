using Extensions;
using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class FunctionBody : IFunctionBody, ILexicalScope
    {
        public string Name { get; }
        public VariableValue? Return { get; set; } = null;
        public List<(VariableValue Name, VariableValue Type)> Arguments { get; } = new List<(VariableValue, VariableValue)>();
        public List<IOperand> Body { get; } = new List<IOperand>();
        public INamespace Namespace { get; }
        public ILexicalScope? Parent { get; } = null;
        public Dictionary<string, ITypedValue> LexicalScope { get; } = new Dictionary<string, ITypedValue>();
        public Dictionary<ITypedValue, VariableDetail> TypeMapper { get; } = new Dictionary<ITypedValue, VariableDetail>();

        public FunctionBody(INamespace ns, string name)
        {
            Namespace = ns;
            Name = name;
        }

        public override string ToString() => $"sub {Name}({Arguments.Map(x => x.Name.ToString() + " : " + x.Type.ToString()).Join(", ")}){(Return is null ? "" : " " + Return.ToString())}";
    }
}
