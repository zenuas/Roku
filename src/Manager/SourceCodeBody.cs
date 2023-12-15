using System.Collections.Generic;

namespace Roku.Manager;

public class SourceCodeBody : INamespace, IUse
{
    public List<IFunctionName> Functions { get; init; } = [];
    public List<IStructBody> Structs { get; init; } = [];
    public List<ClassBody> Classes { get; init; } = [];
    public List<InstanceBody> Instances { get; init; } = [];
    public List<IManaged> Uses { get; init; } = [];
    public int CoroutineUniqueCount { get; set; } = 0;
}
