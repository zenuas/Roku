namespace Roku.Node
{
    public class DeclareNode : INode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public VariableNode Name { get; set; }
        public ITypeNode Type { get; set; }

        public DeclareNode(VariableNode name, ITypeNode type)
        {
            Name = name;
            Type = type;
        }
    }
}
