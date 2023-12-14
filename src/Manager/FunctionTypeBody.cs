using Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Manager;

public class FunctionTypeBody : IStructBody, IFunctionName
{
    public string Name => ToString();
    public List<IStructBody> Arguments { get; } = [];
    public IStructBody? Return { get; set; } = null;

    public override string ToString() => $"{{{Arguments.Select(x => x.Name).Join(", ")}{(Return is { } r ? $" => {r}" : "")}}}";
}
