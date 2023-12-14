using Roku.Declare;
using System;
using System.Collections.Generic;

namespace Roku.Manager;

public class ClassBody(IManaged ns, string name) : INamespace, IAttachedNamespace
{
    public string Name { get; } = name;
    public IManaged Namespace { get; } = ns;
    public List<TypeGenericsParameter> Generics { get; } = [];
    public List<IFunctionName> Functions { get; } = [];
    public List<IStructBody> Structs { get; } = [];
    public List<ClassBody> Classes { get; } = [];
    public List<InstanceBody> Instances => throw new NotImplementedException();

    public override string ToString() => $"class {Name}";
}
