using Extensions;
using System.Collections.Generic;

namespace Roku.IntermediateCode
{
    public class TypeGenericsValue : ITypeDefinition
    {
        public string Name { get => ToString(); }
        public ITypedValue Type { get; }
        public List<ITypeDefinition> Generics { get; } = new List<ITypeDefinition>();

        public TypeGenericsValue(ITypedValue type)
        {
            Type = type;
        }

        public override string ToString() => $"{Type}<{Generics.Map(x => x.ToString()!).Join(", ")}>";
    }
}
