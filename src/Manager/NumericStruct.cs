using System.Collections.Generic;

namespace Roku.Manager;

public class NumericStruct : IStructBody
{
    public string Name => Value.ToString();
    public int Value { get; set; }
    public List<IStructBody> Types { get; } = new List<IStructBody>();
}
