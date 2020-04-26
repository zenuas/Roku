namespace Roku.Node
{
    public class DeclareNode : Node
    {
        public VariableNode Name { get; set; }
        public TypeNode Type { get; set; }

        public DeclareNode(VariableNode name, TypeNode type)
        {
            Name = name;
            Type = type;
        }
    }
}
