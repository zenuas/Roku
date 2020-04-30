using Roku.Parser;

namespace Roku.Node
{
    public class TokenNode : INode, IEvaluableNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public Token Token { get; set; } = new Token();
    }
}
