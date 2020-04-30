namespace Roku.Node
{
    public class DeclareNode : INode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public VariableNode Name { get; set; }
        public TypeNode Type { get; set; }

        public DeclareNode(VariableNode name, TypeNode type)
        {
            Name = name;
            Type = type;
        }
    }
}
