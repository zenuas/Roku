using System;
using System.Collections.Generic;
using System.Reflection;

namespace Roku.Manager;

public class RootNamespace : INamespace
{
    public List<IFunctionName> Functions { get; } = [];
    public List<IStructBody> Structs { get; } = [];
    public List<ClassBody> Classes { get; } = [];
    public List<InstanceBody> Instances => throw new NotImplementedException();
    public List<Assembly> Assemblies { get; } = [];
    public int AnonymousFunctionUniqueCount { get; set; } = 0;
}
