using Extensions;
using Roku.Node;
using System.Collections.Generic;

namespace Roku.Parser
{
    public partial class Parser
    {
        public Stack<IScopeNode> Scopes { get; } = new Stack<IScopeNode>();

        public static ListNode<T> CreateListNode<T>(params T[] expr) where T : INode => new ListNode<T>().Return(x => x.List.AddRange(expr));

        public static VariableNode CreateVariableNode(Token t) => CreateVariableNode(t.Name, t);

        public static VariableNode CreateVariableNode(string s, Token t) => new VariableNode { Name = s }.R(t);

        public static FunctionCallNode CreateFunctionCallNode(IEvaluableNode expr, params IEvaluableNode[] args) => new FunctionCallNode(expr).Return(x => x.Arguments.AddRange(args)).R(expr);

        public static LetNode CreateLetNode(VariableNode v, IEvaluableNode e) => new LetNode(v, e).R(v);
    }
}
