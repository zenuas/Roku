using System.Collections.Generic;

namespace Roku.Manager;

public class SourceCodeBody : INamespace, IUse
{
    public List<IFunctionName> Functions { get; } = [];
    public List<IStructBody> Structs { get; } = [];
    public List<ClassBody> Classes { get; } = [];
    public List<InstanceBody> Instances { get; } = [];
    public List<IManaged> Uses { get; } = [];
    public int CoroutineUniqueCount { get; set; } = 0;
}
