using Extensions;
using Roku.Declare;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class FunctionTypeBody : IStructBody
    {
        public string Name { get => ToString(); }
        public List<ITypeDefinition> Arguments { get; } = new List<ITypeDefinition>();
        public ITypeDefinition? Return { get; set; } = null;

        public override string ToString() => $"{{{Arguments.Map(x => x.Name).Join(", ")}{(Return is { } r ? $" => {r}" : "")}}}";
    }
}
