using Extensions;
using System.Collections.Generic;

namespace Roku.IntermediateCode
{
    public class TypeGenericsValue : ITypeDefinition
    {
        public string Name { get; }
        public Types Types { get; set; } = Types.Struct;
        public List<ITypeDefinition> Generics { get; } = new List<ITypeDefinition>();

        public TypeGenericsValue(string name)
        {
            Name = name;
        }

        public override string ToString() => $"{Name}<{Generics.Map(x => x.ToString()!).Join(", ")}>";
    }
}
