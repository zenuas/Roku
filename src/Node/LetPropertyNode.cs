namespace Roku.Node
{
    public class LetPropertyNode : INode, IStatementNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public IEvaluableNode Left { get; }
        public VariableNode Right { get; }
        public IEvaluableNode Expression { get; set; }

        public LetPropertyNode(IEvaluableNode left, VariableNode right, IEvaluableNode e)
        {
            Left = left;
            Right = right;
            Expression = e;
        }
    }
}
