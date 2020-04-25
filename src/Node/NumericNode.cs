namespace Roku.Node
{
    public class NumericNode : Node, IEvaluableNode
    {
        public uint Value { get; set; } = 0u;
        public string Format { get; set; } = "";
    }
}
