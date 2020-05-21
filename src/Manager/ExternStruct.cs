using Extensions;
using Roku.IntermediateCode;
using System.Collections.Generic;
using System.Reflection;

namespace Roku.Manager
{
    public class ExternStruct : IStructBody, ISpecialization
    {
        public string Name { get; }
        public TypeInfo Struct { get; }
        public Assembly Assembly { get; }
        public List<TypeValue> Generics { get; } = new List<TypeValue>();
        public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; } = new Dictionary<GenericsMapper, TypeMapper>();

        public ExternStruct(string name, TypeInfo f, Assembly asm)
        {
            Name = name;
            Struct = f;
            Assembly = asm;

            f.GenericTypeParameters.Each(x => Generics.Add(new TypeValue(x.Name) { Types = Types.Generics }));
        }

        public override string ToString() => Name;
    }
}
