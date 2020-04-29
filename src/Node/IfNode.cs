namespace Roku.Node
{
    public class IfNode : Node, IStatementNode
    {
        public IEvaluableNode Condition { get; set; }
        public IScopeNode Then { get; set; }
        public IScopeNode? Else { get; set; } = null;

        public IfNode(IEvaluableNode cond, IScopeNode then)
        {
            Condition = cond;
            Then = then;
        }
    }
}
