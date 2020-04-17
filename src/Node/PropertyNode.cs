namespace Roku.Node
{
    public class PropertyNode : Node
    {
        public IEvaluableNode Left { get; set; }
        public VariableNode Right { get; set; }

        public PropertyNode(IEvaluableNode left, VariableNode right)
        {
            Left = left;
            Right = right;
        }
    }
}
