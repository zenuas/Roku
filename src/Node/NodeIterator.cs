using Mina.Extensions;
using System.Collections.Generic;

namespace Roku.Node;

public class NodeIterator
{
    public static IEnumerable<IEvaluableNode> PropertyToList(IEvaluableNode expr) => expr is PropertyNode p ? PropertyToList(p.Left).Concat(p.Right) : new List<IEvaluableNode>() { expr };
}
