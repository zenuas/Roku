using System.Collections.Generic;

namespace Roku.Declare
{
    public class TypeValue : ITypeDefinition
    {
        public List<string> Namespace { get; } = new List<string>();
        public string Name { get; }

        public TypeValue(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;
    }
}
