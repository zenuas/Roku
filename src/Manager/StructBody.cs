using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class StructBody : IStructBody, ILexicalScope, ISpecialization
    {
        public string Name { get; }
        public Dictionary<string, ITypedValue> Members { get; } = new Dictionary<string, ITypedValue>();
        public List<IOperand> Body { get; } = new List<IOperand>();
        public INamespace Namespace { get; }
        public ILexicalScope? Parent { get; } = null;
        public Dictionary<string, ITypedValue> LexicalScope { get; } = new Dictionary<string, ITypedValue>();
        public int MaxTemporaryValue { get; set; } = 0;
        public List<TypeValue> Generics { get; } = new List<TypeValue>();
        public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; } = new Dictionary<GenericsMapper, TypeMapper>();

        public StructBody(INamespace ns, string name)
        {
            Namespace = ns;
            Name = name;
        }

        public override string ToString() => Name;
    }
}
