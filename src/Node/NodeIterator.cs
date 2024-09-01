using Mina.Extension;
using System.Collections.Generic;

namespace Roku.Node;

public class NodeIterator
{
    public static IEnumerable<IEvaluableNode> PropertyToList(IEvaluableNode expr) => expr is PropertyNode p ? PropertyToList(p.Left).Concat(p.Right) : [expr];
}
