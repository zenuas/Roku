using System.Collections.Generic;

namespace Roku.Node
{
    public class TupleNode : INode, IEvaluableNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public List<IEvaluableNode> Values { get; } = new List<IEvaluableNode>();
    }
}
