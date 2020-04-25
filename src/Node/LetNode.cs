namespace Roku.Node
{
    public class LetNode : Node, IStatementNode
    {
        public VariableNode Var { get; }
        public IEvaluableNode Expression { get; set; }

        public LetNode(VariableNode v, IEvaluableNode e)
        {
            Var = v;
            Expression = e;
        }
    }
}
