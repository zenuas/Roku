using Roku.Declare;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class ClassBody : INamespaceBody
    {
        public string Name { get; }
        public INamespace Namespace { get; }
        public ILexicalScope? Parent { get; } = null;
        public List<TypeGenericsParameter> Generics { get; } = new List<TypeGenericsParameter>();
        public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; } = new Dictionary<GenericsMapper, TypeMapper>();
        public List<IFunctionName> Functions { get; } = new List<IFunctionName>();
        public List<IStructBody> Structs { get; } = new List<IStructBody>();
        public List<ClassBody> Classes { get; } = new List<ClassBody>();

        public ClassBody(INamespace ns, string name)
        {
            Namespace = ns;
            Name = name;
        }

        public override string ToString() => $"class {Name}";
    }
}
