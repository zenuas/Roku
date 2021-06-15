using Roku.Declare;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class ClassBody
    {
        public string Name { get; }
        public INamespace Namespace { get; }
        public ILexicalScope? Parent { get; } = null;
        public List<TypeGenericsParameter> Generics { get; } = new List<TypeGenericsParameter>();
        public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; } = new Dictionary<GenericsMapper, TypeMapper>();
        public List<FunctionBody> Classes { get; } = new List<FunctionBody>();

        public ClassBody(INamespace ns, string name)
        {
            Namespace = ns;
            Name = name;
        }

        public override string ToString() => $"class {Name}";
    }
}
