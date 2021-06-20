using Extensions;
using Roku.Declare;
using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class FunctionBody : IFunctionBody, ILexicalScope, IConstraints
    {
        public string Name { get; }
        public ITypeDefinition? Return { get; set; } = null;
        public List<(VariableValue Name, ITypeDefinition Type)> Arguments { get; } = new List<(VariableValue, ITypeDefinition)>();
        public List<IOperand> Body { get; } = new List<IOperand>();
        public INamespace Namespace { get; }
        public ILexicalScope? Parent { get; } = null;
        public Dictionary<string, IEvaluable> LexicalScope { get; } = new Dictionary<string, IEvaluable>();
        public int MaxTemporaryValue { get; set; } = 0;
        public List<TypeGenericsParameter> Generics { get; } = new List<TypeGenericsParameter>();
        public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; } = new Dictionary<GenericsMapper, TypeMapper>();
        public List<(VariableValue Class, List<TypeGenericsParameter> Generics)> Constraints { get; } = new List<(VariableValue, List<TypeGenericsParameter>)>();

        public FunctionBody(INamespace ns, string name)
        {
            Namespace = ns;
            Name = name;
        }

        public override string ToString() => $"sub {Name}({Arguments.Map(x => x.Name.ToString() + " : " + x.Type.ToString()).Join(", ")}){(Return is null ? "" : " " + Return.ToString())}";
    }
}
