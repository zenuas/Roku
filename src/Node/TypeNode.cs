namespace Roku.Node
{
    public class TypeNode : INode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public string Name { get; set; } = "";
    }
}
