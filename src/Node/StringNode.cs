namespace Roku.Node
{
    public class StringNode : INode, IEvaluableNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public string Value { get; set; } = "";
    }
}
