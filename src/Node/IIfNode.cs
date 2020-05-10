using System.Collections.Generic;

namespace Roku.Node
{
    public interface IIfNode : INode
    {
        public IScopeNode Then { get; set; }
        public List<IIfNode> ElseIf { get; }
        public IScopeNode? Else { get; set; }
    }
}
