using Roku.Declare;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Roku.Manager;

public class ExternStruct : IStructBody, ISpecialization, INamespaceBody
{
    public string Name { get; set; }
    public TypeInfo Struct { get; }
    public Assembly Assembly { get; }
    public List<TypeGenericsParameter> Generics { get; } = new List<TypeGenericsParameter>();
    public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; } = new Dictionary<GenericsMapper, TypeMapper>();
    public List<IFunctionName> Functions { get; } = new List<IFunctionName>();
    public List<IStructBody> Structs => throw new NotImplementedException();
    public List<ClassBody> Classes => throw new NotImplementedException();
    public List<InstanceBody> Instances => throw new NotImplementedException();

    public ExternStruct(TypeInfo ti, Assembly asm)
    {
        Struct = ti;
        Assembly = asm;
        Name = $"###{ti.FullName}";
    }

    public override string ToString() => Name;
}
