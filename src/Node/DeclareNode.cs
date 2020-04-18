namespace Roku.Node
{
    public class DeclareNode : Node
    {
        public VariableNode Name { get; set; }
        public VariableNode Type { get; set; }

        public DeclareNode(VariableNode name, VariableNode type)
        {
            Name = name;
            Type = type;
        }
    }
}
