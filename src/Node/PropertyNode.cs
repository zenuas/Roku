namespace Roku.Node
{
    public class PropertyNode : INode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public IEvaluableNode Left { get; set; }
        public VariableNode Right { get; set; }

        public PropertyNode(IEvaluableNode left, VariableNode right)
        {
            Left = left;
            Right = right;
        }
    }
}
