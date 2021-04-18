using Extensions;
using Roku.Declare;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class FunctionTypeBody : IStructBody
    {
        public string Name { get => ToString(); }
        public List<IStructBody> Arguments { get; } = new List<IStructBody>();
        public IStructBody? Return { get; set; } = null;

        public override string ToString() => $"{{{Arguments.Map(x => x.Name).Join(", ")}{(Return is { } r ? $" => {r}" : "")}}}";
    }
}
