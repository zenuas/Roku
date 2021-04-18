using Roku.Declare;
using System.Collections.Generic;
using System.Reflection;

namespace Roku.Manager
{
    public class ExternStruct : IStructBody, ISpecialization, INamespaceBody
    {
        public string Name { get; set; } = "###NO-ALIAS";
        public TypeInfo Struct { get; }
        public Assembly Assembly { get; }
        public List<TypeGenericsParameter> Generics { get; } = new List<TypeGenericsParameter>();
        public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; } = new Dictionary<GenericsMapper, TypeMapper>();
        public List<IFunctionName> Functions { get; } = new List<IFunctionName>();
        public List<IStructBody> Structs { get; } = new List<IStructBody>();

        public ExternStruct(TypeInfo ti, Assembly asm)
        {
            Struct = ti;
            Assembly = asm;
        }

        public override string ToString() => Name;
    }
}
