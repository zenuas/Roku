using System.Collections.Generic;

namespace Roku.Manager;

public class NumericStruct : IStructBody
{
    public string Name => Value.ToString();
    public int Value { get; init; }
    public List<IStructBody> Types { get; init; } = [];
}
