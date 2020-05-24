using Roku.IntermediateCode;
using System.Collections.Generic;
using System.Reflection;

namespace Roku.Manager
{
    public class ExternStruct : IStructBody, ISpecialization, INamespace
    {
        public string Name { get; set; } = "###NO-ALIAS";
        public TypeInfo Struct { get; }
        public Assembly Assembly { get; }
        public List<TypeValue> Generics { get; } = new List<TypeValue>();
        public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; } = new Dictionary<GenericsMapper, TypeMapper>();
        public INamespace? Parent => null;
        public List<IFunctionBody> Functions { get; } = new List<IFunctionBody>();
        public List<IStructBody> Structs { get; } = new List<IStructBody>();

        public ExternStruct(TypeInfo ti, Assembly asm)
        {
            Struct = ti;
            Assembly = asm;
        }

        public override string ToString() => Name;
    }
}
