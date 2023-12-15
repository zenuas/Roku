using Roku.Declare;
using System;
using System.Collections.Generic;

namespace Roku.Manager;

public class ClassBody : INamespace, IAttachedNamespace
{
    public required string Name { get; init; }
    public required IManaged Namespace { get; init; }
    public List<TypeGenericsParameter> Generics { get; } = [];
    public List<IFunctionName> Functions { get; } = [];
    public List<IStructBody> Structs { get; } = [];
    public List<ClassBody> Classes { get; } = [];
    public List<InstanceBody> Instances => throw new NotImplementedException();

    public override string ToString() => $"class {Name}";
}
