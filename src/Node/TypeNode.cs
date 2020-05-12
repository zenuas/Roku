namespace Roku.Node
{
    public class TypeNode : INode, ITypeNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public string Name { get; set; } = "";
    }
}
