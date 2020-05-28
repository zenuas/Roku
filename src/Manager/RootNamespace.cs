using System.Collections.Generic;
using System.Reflection;

namespace Roku.Manager
{
    public class RootNamespace : INamespace
    {
        public List<IFunctionBody> Functions { get; } = new List<IFunctionBody>();
        public List<IStructBody> Structs { get; } = new List<IStructBody>();
        public List<Assembly> Assemblies { get; } = new List<Assembly>();
    }
}
