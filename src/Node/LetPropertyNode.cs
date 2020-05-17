namespace Roku.Node
{
    public class LetPropertyNode : INode, IStatementNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public IEvaluableNode Reciever { get; }
        public VariableNode Name { get; }
        public IEvaluableNode Expression { get; set; }

        public LetPropertyNode(IEvaluableNode recv, VariableNode name, IEvaluableNode e)
        {
            Reciever = recv;
            Name = name;
            Expression = e;
        }
    }
}
