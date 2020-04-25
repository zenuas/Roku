using System.Collections.Generic;

namespace Roku.Node
{
    public class ProgramNode : Node, IScopeNode
    {
        public List<VariableNode> Uses { get; } = new List<VariableNode>();
        public string FileName { get; set; } = "";
        public List<IStatementNode> Statements { get; } = new List<IStatementNode>();
        public List<FunctionNode> Functions { get; } = new List<FunctionNode>();
    }
}
