using Roku.Declare;
using System;
using System.Collections.Generic;

namespace Roku.Manager;

public class ClassBody : INamespaceBody, IAttachedNamespace
{
    public string Name { get; }
    public INamespace Namespace { get; }
    public List<TypeGenericsParameter> Generics { get; } = new List<TypeGenericsParameter>();
    public List<IFunctionName> Functions { get; } = new List<IFunctionName>();
    public List<IStructBody> Structs { get; } = new List<IStructBody>();
    public List<ClassBody> Classes { get; } = new List<ClassBody>();
    public List<InstanceBody> Instances => throw new NotImplementedException();

    public ClassBody(INamespace ns, string name)
    {
        Namespace = ns;
        Name = name;
    }

    public override string ToString() => $"class {Name}";
}
