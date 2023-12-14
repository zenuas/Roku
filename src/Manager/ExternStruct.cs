using Roku.Declare;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Roku.Manager;

public class ExternStruct(TypeInfo ti, Assembly asm) : IStructBody, ISpecialization, INamespace
{
    public string Name { get; set; } = $"###{ti.FullName}";
    public TypeInfo Struct { get; } = ti;
    public Assembly Assembly { get; } = asm;
    public List<TypeGenericsParameter> Generics { get; } = [];
    public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; } = [];
    public List<IFunctionName> Functions { get; } = [];
    public List<IStructBody> Structs => throw new NotImplementedException();
    public List<ClassBody> Classes => throw new NotImplementedException();
    public List<InstanceBody> Instances => throw new NotImplementedException();

    public override string ToString() => Name;
}
