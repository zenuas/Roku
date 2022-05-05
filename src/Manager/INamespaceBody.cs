using System.Collections.Generic;

namespace Roku.Manager;

public interface INamespaceBody : IManaged
{
    public List<IFunctionName> Functions { get; }
    public List<IStructBody> Structs { get; }
    public List<ClassBody> Classes { get; }
    public List<InstanceBody> Instances { get; }
}
