using System.Collections.Generic;

namespace Roku.Node
{
    public class ProgramNode
    {
        public List<string> Uses { get; } = new List<string>();
        public string FileName { get; set; } = "";
        public List<IStatementNode> Statements { get; } = new List<IStatementNode>();
    }
}
