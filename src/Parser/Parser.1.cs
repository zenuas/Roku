using Extensions;
using Roku.Node;
using System;
using System.Collections.Generic;

namespace Roku.Parser
{
    public partial class Parser
    {
        public Stack<IScopeNode> Scopes { get; } = new Stack<IScopeNode>();

        public static ListNode<T> CreateListNode<T>(params T[] expr) where T : INode => new ListNode<T>().Return(x => x.List.AddRange(expr));

        public static VariableNode CreateVariableNode(Token t) => CreateVariableNode(t.Name).R(t);

        public static VariableNode CreateVariableNode(string s) => new VariableNode { Name = s };

        public static FunctionCallNode CreateFunctionCallNode(Token token, params IEvaluableNode[] args) => new FunctionCallNode(CreateVariableNode(token)).Return(x => x.Arguments.AddRange(args)).R(token);

        public static PropertyNode CreatePropertyNode(IEvaluableNode left, VariableNode right) => new PropertyNode(left, right);

        public static FunctionCallNode CreateFunctionCallNode(IEvaluableNode expr, params IEvaluableNode[] args) => new FunctionCallNode(expr).Return(x => x.Arguments.AddRange(args)).R(expr);

        public static LetNode CreateLetNode(VariableNode v, IEvaluableNode e) => new LetNode(v, e).R(v);

        public static LetPropertyNode CreateLetNode(IEvaluableNode left, VariableNode right, IEvaluableNode e) => new LetPropertyNode(left, right, e).R(right);

        public static LetTypeNode CreateLetNode(VariableNode v, ITypeNode t) => new LetTypeNode(v, t).R(v);

        public static FunctionNode CreateFunctionNode(
                FunctionNode fn,
                VariableNode name,
                ListNode<DeclareNode> args,
                ITypeNode? ret,
                INode where
            )
        {
            fn.Name = name;
            fn.Arguments.AddRange(args.List);
            fn.Return = ret;
            return fn.R(name);
        }

        public static IfNode CreateIfNode(IEvaluableNode cond, IScopeNode then) => new IfNode(cond, then).R(cond);

        public static IfCastNode CreateIfCastNode(VariableNode name, ITypeNode declare, IEvaluableNode cond, IScopeNode then) => new IfCastNode(name, declare, cond, then).R(cond);

        public static IIfNode AddElse(IIfNode if_, IScopeNode else_) => if_.Return(x => x.Else = else_);

        public static BlockNode ToBlock(IStatementNode stmt) => new BlockNode().Return(x => x.Statements.Add(stmt)).R(stmt);

        public static BlockNode ToStatementBlock(IEvaluableNode expr) => ToBlock(expr.Cast<IStatementNode>());

        public static ITypeNode ExpressionToType(IEvaluableNode expr)
        {
            return expr switch
            {
                VariableNode v => new TypeNode { Name = v.Name },
                ITypeNode t => t,
                _ => throw new SyntaxErrorException("not type"),
            };
        }

        public static TypeGenericsNode ExpressionToType(IEvaluableNode expr, IEvaluableNode t1, params ITypeNode[] ts)
        {
            var g = new TypeGenericsNode(expr);
            g.Generics.Add(ExpressionToType(t1));
            g.Generics.AddRange(ts);
            return g;
        }

        public void SyntaxError(Token t) => SyntaxError(t, "syntax error");

        public static void SyntaxError(Token t, string message) => throw new SyntaxErrorException(message) { LineNumber = t.LineNumber, LineColumn = t.LineColumn };

        public static void SyntaxError(INode node, string message) => throw new SyntaxErrorException(message) { LineNumber = node.LineNumber, LineColumn = node.LineColumn };
    }
}
