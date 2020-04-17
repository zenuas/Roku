namespace Roku.Node
{
    public abstract class Node : INode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
    }
}
