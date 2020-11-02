using Extensions;
using System.Collections.Generic;

namespace Roku.Declare
{
    public class TypeFunction : ITypeDefinition
    {
        public string Name { get => ToString(); }
        public List<ITypeDefinition> Arguments { get; } = new List<ITypeDefinition>();
        public ITypeDefinition? Return { get; set; } = null;

        public override string ToString() => $"{{{Arguments.Map(x => x.ToString()!).Join(", ")}{(Return is { } r ? $" => {r}" : "")}}}";
    }
}
