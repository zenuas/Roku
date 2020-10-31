namespace Roku.Node
{
    public interface IDeclareNode : INode
    {
        public VariableNode Name { get; set; }
    }
}
