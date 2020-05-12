namespace Roku.Node
{
    public class TypeArrayNode : INode, ITypeNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public ITypeNode Item { get; }

        public TypeArrayNode(ITypeNode item)
        {
            Item = item;
        }
    }
}
