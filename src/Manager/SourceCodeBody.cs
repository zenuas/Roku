using System.Collections.Generic;

namespace Roku.Manager;

public class SourceCodeBody : INamespace, IUse
{
    public List<IFunctionName> Functions { get; } = new List<IFunctionName>();
    public List<IStructBody> Structs { get; } = new List<IStructBody>();
    public List<ClassBody> Classes { get; } = new List<ClassBody>();
    public List<InstanceBody> Instances { get; } = new List<InstanceBody>();
    public List<IManaged> Uses { get; } = new List<IManaged>();
    public int CoroutineUniqueCount { get; set; } = 0;
}
