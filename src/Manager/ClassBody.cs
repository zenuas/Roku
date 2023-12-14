using Roku.Declare;
using System;
using System.Collections.Generic;

namespace Roku.Manager;

public class ClassBody : INamespace, IAttachedNamespace
{
    public string Name { get; }
    public IManaged Namespace { get; }
    public List<TypeGenericsParameter> Generics { get; } = [];
    public List<IFunctionName> Functions { get; } = [];
    public List<IStructBody> Structs { get; } = [];
    public List<ClassBody> Classes { get; } = [];
    public List<InstanceBody> Instances => throw new NotImplementedException();

    public ClassBody(IManaged ns, string name)
    {
        Namespace = ns;
        Name = name;
    }

    public override string ToString() => $"class {Name}";
}
