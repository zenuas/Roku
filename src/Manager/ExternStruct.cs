using Roku.Declare;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Roku.Manager;

public class ExternStruct(TypeInfo ti, Assembly asm) : IStructBody, ISpecialization, INamespace
{
    public string Name { get; set; } = $"###{ti.FullName}";
    public TypeInfo Struct { get; init; } = ti;
    public Assembly Assembly { get; init; } = asm;
    public List<TypeGenericsParameter> Generics { get; init; } = [];
    public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; init; } = [];
    public List<IFunctionName> Functions { get; init; } = [];
    public List<IStructBody> Structs => throw new NotImplementedException();
    public List<ClassBody> Classes => throw new NotImplementedException();
    public List<InstanceBody> Instances => throw new NotImplementedException();

    public override string ToString() => Name;
}
