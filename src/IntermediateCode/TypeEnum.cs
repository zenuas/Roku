using Extensions;
using System.Collections.Generic;

namespace Roku.IntermediateCode
{
    public class TypeEnum : ITypeDefinition
    {
        public string Name { get => ToString(); }
        public List<ITypeDefinition> Enums { get; } = new List<ITypeDefinition>();

        public TypeEnum(IEnumerable<ITypeDefinition> enums)
        {
            Enums.AddRange(enums);
        }

        public override string ToString() => $"[{Enums.Map(x => x.ToString()!).Join(" | ")}]";
    }
}
