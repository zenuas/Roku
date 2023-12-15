using System;
using System.Collections.Generic;
using System.Reflection;

namespace Roku.Manager;

public class RootNamespace : INamespace
{
    public List<IFunctionName> Functions { get; init; } = [];
    public List<IStructBody> Structs { get; init; } = [];
    public List<ClassBody> Classes { get; init; } = [];
    public List<InstanceBody> Instances => throw new NotImplementedException();
    public List<Assembly> Assemblies { get; init; } = [];
    public int AnonymousFunctionUniqueCount { get; set; } = 0;
}
