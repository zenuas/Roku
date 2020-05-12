using System.Collections.Generic;

namespace Roku.Node
{
    public class BlockNode : INode, IScopeNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public List<IStatementNode> Statements { get; } = new List<IStatementNode>();
        public List<FunctionNode> Functions { get; } = new List<FunctionNode>();
        public List<StructNode> Structs { get; } = new List<StructNode>();
    }
}
