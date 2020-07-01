using Extensions;
using System.Collections.Generic;

namespace Roku.Node
{
    public class EnumNode : INode, ITypeNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public List<ITypeNode> Types { get; } = new List<ITypeNode>();
        public string Name => $"[{Types.Map(x => x.Name).Join(" | ")}]";

        public EnumNode(ListNode<ITypeNode> types)
        {
            Types = types.List;
        }
    }
}
