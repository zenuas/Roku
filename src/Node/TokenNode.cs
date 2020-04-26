using Roku.Parser;

namespace Roku.Node
{
    public class TokenNode : Node, IEvaluableNode
    {
        public Token Token { get; set; } = new Token();
    }
}
