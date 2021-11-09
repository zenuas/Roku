using System.Collections.Generic;

namespace Roku.Node;

public class InstanceNode : INode
{
    public int? LineNumber { get; set; }
    public int? LineColumn { get; set; }
    public ITypeNode Type { get; }
    public SpecializationNode Specialization { get; }
    public List<InstanceMapNode> InstanceMap { get; } = new List<InstanceMapNode>();

    public InstanceNode(
            ITypeNode type,
            SpecializationNode spec,
            ListNode<InstanceMapNode> maps
        )
    {
        Type = type;
        Specialization = spec;
        InstanceMap = maps.List;
    }
}
