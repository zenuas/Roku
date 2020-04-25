using System.Collections.Generic;

namespace Roku.Node
{
    public class ListNode<T> : Node, IEvaluableNode
        where T : INode
    {
        public List<T> List { get; } = new List<T>();
    }
}
