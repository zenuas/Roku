using System.Collections.Generic;
using System.Reflection;

namespace Roku.Manager
{
    public class RootNamespace : INamespaceBody
    {
        public List<IFunctionName> Functions { get; } = new List<IFunctionName>();
        public List<IStructBody> Structs { get; } = new List<IStructBody>();
        public List<ClassBody> Classes { get; } = new List<ClassBody>();
        public List<Assembly> Assemblies { get; } = new List<Assembly>();
        public int AnonymousFunctionUniqueCount { get; set; } = 0;
    }
}
