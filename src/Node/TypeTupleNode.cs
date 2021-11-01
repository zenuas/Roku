using Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Node
{
    public class TypeTupleNode : INode, ITypeNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public List<ITypeNode> Types { get; } = new List<ITypeNode>();
        public string Name => $"[{Types.Select(x => x.Name).Join(", ")}]";

        public TypeTupleNode(ListNode<ITypeNode> types)
        {
            Types = types.List;
        }
    }
}
