using Extensions;
using Roku.Node;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Parser;

public partial class Parser
{
    public Stack<IScopeNode> Scopes { get; } = new Stack<IScopeNode>();

    public static ListNode<T> CreateListNode<T>(params T[] expr) where T : INode => new ListNode<T>().Return(x => x.List.AddRange(expr));

    public static VariableNode CreateVariableNode(TokenNode t) => CreateVariableNode(t.Name).R(t);

    public static VariableNode CreateVariableNode(string s) => new VariableNode { Name = s };

    public static FunctionCallNode CreateFunctionCallNode(TokenNode token, params IEvaluableNode[] args) => new FunctionCallNode { Expression = CreateVariableNode(token) }.Return(x => x.Arguments.AddRange(args)).R(token);

    public static PropertyNode CreatePropertyNode(IEvaluableNode left, VariableNode right) => new PropertyNode(left, right);

    public static FunctionCallNode CreateFunctionCallNode(IEvaluableNode expr, params IEvaluableNode[] args) => new FunctionCallNode { Expression = expr }.Return(x => x.Arguments.AddRange(args)).R(expr);

    public static LetIgnoreNode CreateLetIgnoreNode(TokenNode t) => new LetIgnoreNode().R(t);

    public static LetVarNode CreateLetNode(VariableNode v) => new LetVarNode { Var = v }.R(v);

    public static LetNode CreateLetNode(VariableNode v, IEvaluableNode e) => new LetNode { Var = v, Expression = e }.R(v);

    public static LetPropertyNode CreateLetNode(IEvaluableNode left, VariableNode right, IEvaluableNode e) => new LetPropertyNode { Reciever = left, Name = right, Expression = e }.R(right);

    public static LetTypeNode CreateLetNode(VariableNode v, ITypeNode t) => new LetTypeNode { Var = v, Type = t }.R(v);

    public static LetTupleAssignmentNode CreateLetTupleNode(ListNode<ITupleBind> v, IEvaluableNode e) => new LetTupleAssignmentNode { Expression = e }.Return(x => x.Assignment.AddRange(v.List)).R(v);

    public static TupleNode CreateTupleNode(ListNode<IEvaluableNode> v) => new TupleNode().Return(x => x.Values.AddRange(v.List));

    public static ClassNode CreateClassNode(VariableNode v, ListNode<ITypeNode> gen, ListNode<FunctionNode> condn)
    {
        var class_ = new ClassNode() { Name = v }.R(v);
        class_.Generics.AddRange(gen.List);
        class_.Functions.AddRange(condn.List);
        return class_;
    }

    public static InstanceNode CreateInstanceNode(
            SpecializationNode spec,
            ListNode<InstanceMapNode> maps
        ) => new InstanceNode { Specialization = spec, InstanceMap = maps.List }.R(spec);

    public static InstanceMapNode CreateInstanceMapNode(
            VariableNode name,
            ListNode<IDeclareNode> args,
            LambdaExpressionNode stmt
        )
    {
        var m = new InstanceMapNode().R(name);
        m.Name = name;
        m.Arguments.AddRange(args.List);
        m.Statements.AddRange(stmt.Statements);
        return m;
    }

    public static FunctionNode CreateFunctionNode(
            FunctionNode fn,
            VariableNode name,
            ListNode<DeclareNode> args,
            ITypeNode? ret,
            ListNode<SpecializationNode> where
        )
    {
        fn.Name = name;
        fn.Arguments.AddRange(args.List);
        fn.Return = ret;
        fn.Constraints.AddRange(where.List);
        return fn.R(name);
    }

    public static FunctionNode CreateFunctionNode(
            VariableNode name,
            ListNode<DeclareNode> args,
            ITypeNode? ret,
            ListNode<SpecializationNode> where
        ) => CreateFunctionNode(new FunctionNode().R(name), name, args, ret, where);

    public static FunctionNode CreateFunctionNode(
            VariableNode name,
            ListNode<ITypeNode> args,
            ITypeNode? ret,
            ListNode<SpecializationNode> where
        ) => CreateFunctionNode(name, new ListNode<DeclareNode>().Return(x => x.List.AddRange(args.List.Select((y, i) => new DeclareNode { Name = CreateVariableNode($"arg{i}"), Type = y }))), ret, where);

    public static LambdaExpressionNode CreateLambdaFunction(LambdaExpressionNode lambda, ListNode<IDeclareNode> args, ITypeNode? ret, bool isimplicit)
    {
        lambda.Arguments.AddRange(args.List);
        lambda.Return = ret;
        lambda.IsImplicit = isimplicit;
        return lambda;
    }

    public static LambdaExpressionNode ToLambdaExpression(IEvaluableNode expr) => new LambdaExpressionNode().Return(x => x.Statements.Add(new ImplicitReturn { Expression = expr }));

    public static IfNode CreateIfNode(IEvaluableNode cond, IScopeNode then) => new IfNode { Condition = cond, Then = then }.R(cond);

    public static IfCastNode CreateIfCastNode(VariableNode name, ITypeNode declare, IEvaluableNode cond, IScopeNode then) => new IfCastNode { Name = name, Declare = declare, Condition = cond, Then = then }.R(cond);

    public static IfArrayCastNode CreateIfArrayCastNode(ListNode<VariableNode> array_pattern, IEvaluableNode cond, IScopeNode then) => new IfArrayCastNode { ArrayPattern = array_pattern, Condition = cond, Then = then }.R(cond);

    public static IIfNode AddElse(IIfNode if_, IScopeNode else_) => if_.Return(x => x.Else = else_);

    public static TypeStructNode CreateTypeStructNode(VariableNode name, ListNode<DeclareNode> args) => new TypeStructNode() { StructName = name }.Return(x => x.Arguments.AddRange(args.List)).R(name);

    public static TypeFunctionNode CreateTypeFunctionNode(ListNode<ITypeNode> args, ITypeNode? ret = null) => new TypeFunctionNode() { Return = ret }.Return(x => x.Arguments.AddRange(args.List));

    public static BlockNode ToBlock(IStatementNode stmt) => new BlockNode().Return(x => x.Statements.Add(stmt)).R(stmt);

    public static BlockNode ToStatementBlock(IEvaluableNode expr) => ToBlock(expr.Cast<IStatementNode>());

    public static ListNode<VariableNode> ToArrayPattern(ListNode<IEvaluableNode> list) => list.List.All(x => x is VariableNode) ? CreateListNode(list.List.OfType<VariableNode>().ToArray()) : throw new SyntaxErrorException("not array-pattern");

    public static ITypeNode ExpressionToType(IEvaluableNode expr)
    {
        return expr switch
        {
            VariableNode v => new TypeNode { Name = v.Name },
            ITypeNode t => t,
            _ => throw new SyntaxErrorException("not type"),
        };
    }

    public static SpecializationNode CreateSpecialization(IEvaluableNode expr, IEvaluableNode t1, params ITypeNode[] ts)
    {
        var g = new SpecializationNode(expr);
        g.Generics.Add(ExpressionToType(t1));
        g.Generics.AddRange(ts);
        return g;
    }

    public static SpecializationNode CreateSpecialization(IEvaluableNode expr, ListNode<ITypeNode> ts)
    {
        var g = new SpecializationNode(expr);
        g.Generics.AddRange(ts.List);
        return g;
    }

    public static EnumNode CreateNullable(ITypeNode type) =>
        type is EnumNode e
        ? e.Return(x => x.Types.Add(new TypeNode() { Name = "Null" }.R(type)))
        : new EnumNode { Types = CreateListNode(type, new TypeNode() { Name = "Null" }.R(type)).List };

    public void SyntaxError(IToken<INode> t) => SyntaxError(t.Value, "syntax error");

    public static void SyntaxError(INode node, string message) => throw new SyntaxErrorException(message) { LineNumber = node.LineNumber, LineColumn = node.LineColumn };
}
