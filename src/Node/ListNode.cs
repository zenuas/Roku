using System.Collections.Generic;

namespace Roku.Node;

public class ListNode<T> : INode, IEvaluableNode
    where T : INode
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public List<T> List { get; } = new List<T>();
}
