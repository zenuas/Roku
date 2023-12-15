using Roku.Declare;
using System;
using System.Collections.Generic;

namespace Roku.Manager;

public class ClassBody : INamespace, IAttachedNamespace
{
    public required string Name { get; init; }
    public required IManaged Namespace { get; init; }
    public List<TypeGenericsParameter> Generics { get; init; } = [];
    public List<IFunctionName> Functions { get; init; } = [];
    public List<IStructBody> Structs { get; init; } = [];
    public List<ClassBody> Classes { get; init; } = [];
    public List<InstanceBody> Instances => throw new NotImplementedException();

    public override string ToString() => $"class {Name}";
}
