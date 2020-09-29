using Extensions;
using Roku.Declare;
using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class FunctionBody : IFunctionBody, ILexicalScope, ISpecialization, INamespace
    {
        public string Name { get; }
        public ITypeDefinition? Return { get; set; } = null;
        public List<(VariableValue Name, ITypeDefinition Type)> Arguments { get; } = new List<(VariableValue, ITypeDefinition)>();
        public List<IOperand> Body { get; } = new List<IOperand>();
        public INamespace Namespace { get; }
        public ILexicalScope? Parent { get; } = null;
        public Dictionary<string, ITypedValue> LexicalScope { get; } = new Dictionary<string, ITypedValue>();
        public int MaxTemporaryValue { get; set; } = 0;
        public List<TypeGenericsParameter> Generics { get; } = new List<TypeGenericsParameter>();
        public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; } = new Dictionary<GenericsMapper, TypeMapper>();
        public List<IFunctionBody> Functions { get; } = new List<IFunctionBody>();
        public List<IStructBody> Structs { get; } = new List<IStructBody>();

        public FunctionBody(INamespace ns, string name)
        {
            Namespace = ns;
            Name = name;
        }

        public override string ToString() => $"sub {Name}({Arguments.Map(x => x.Name.ToString() + " : " + x.Type.ToString()).Join(", ")}){(Return is null ? "" : " " + Return.ToString())}";
    }
}
