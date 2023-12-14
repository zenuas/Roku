using Extensions;
using Roku.Declare;
using Roku.IntermediateCode;
using Roku.Manager;
using Roku.Node;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Compiler;

public static partial class Definition
{
    public static void FunctionDefinition(SourceCodeBody src, IScopeNode scope)
    {
        FunctionBody? main = null;
        if (scope.Statements.Count > 0)
        {
            main = MakeFunction(src, "main");
            main.SpecializationMapper[[]] = [];
            FunctionBodyDefinition(main, scope.Statements);
        }

        scope.Functions.Each(f => MakeFunctionBodyDefinition(src, f, main));
    }

    public static FunctionBody MakeFunctionBodyDefinition(INamespace ns, FunctionNode f, ILexicalScope? parent = null)
    {
        var body = MakeFunctionDefinition(ns, null, f, parent);
        FunctionBodyDefinition(body, f.Statements);

        if (body.Body.Contains(IsYield)) ConvertCoroutine(ns, body);
        return body;
    }

    public static void FunctionBodyDefinition(ILexicalScope scope, List<IStatementNode> stmts)
    {
        stmts.Each(stmt =>
        {
            switch (stmt)
            {
                case LetNode let:
                    {
                        var v = new VariableValue() { Name = let.Var.Name };
                        scope.LexicalScope.Add(let.Var.Name, v);
                        var e = NormalizationExpression(scope, let.Expression);
                        if (e is FunctionCallValue fcall)
                        {
                            scope.Body.Add(new Call(fcall) { Return = v });
                        }
                        else
                        {
                            scope.Body.Add(new BindCode { Return = v, Value = e });
                        }
                    }
                    break;

                case LetTypeNode let:
                    {
                        var v = new VariableValue() { Name = let.Var.Name };
                        scope.LexicalScope.Add(let.Var.Name, v);
                        scope.Body.Add(new TypeBind(v, CreateType(scope, let.Type)));
                    }
                    break;

                case LetPropertyNode let:
                    scope.Body.Add(new BindCode { Return = new PropertyValue(NormalizationExpression(scope, let.Reciever, true), let.Name.Name), Value = NormalizationExpression(scope, let.Expression, true) });
                    break;

                case LetTupleAssignmentNode let:
                    {
                        var e = NormalizationExpression(scope, let.Expression, true);
                        let.Assignment.Each((x, i) =>
                        {
                            if (x is LetVarNode letv)
                            {
                                var v = new VariableValue() { Name = letv.Var.Name };
                                scope.LexicalScope.Add(letv.Var.Name, v);
                                scope.Body.Add(new BindCode { Return = v, Value = new PropertyValue(e, $"{i + 1}") });
                            }
                        });
                    }
                    break;

                case FunctionCallNode call:
                    scope.Body.Add(new Call(NormalizationExpression(scope, call).Cast<FunctionCallValue>()));
                    break;

                case IIfNode if_:
                    IfBodyDefinition(scope, if_);
                    break;

                case ImplicitReturn imp:
                    {
                        var v = new ImplicitReturnValue();
                        scope.LexicalScope.Add(v.Name, v);
                        var e = NormalizationExpression(scope, imp.Expression);
                        if (e is FunctionCallValue fcall)
                        {
                            scope.Body.Add(new Call(fcall) { Return = v });
                        }
                        else
                        {
                            scope.Body.Add(new BindCode { Return = v, Value = e });
                        }
                    }
                    break;

                default:
                    throw new();
            }
        });
    }

    public static FunctionBody MakeFunction(INamespace ns, string name, ILexicalScope? parent = null)
    {
        var body = new FunctionBody(ns, name, parent);
        ns.Functions.Add(body);
        return body;
    }

    public static FunctionBody MakeFunctionDefinition(INamespace ns, List<TypeGenericsParameter>? gens, FunctionNode f, ILexicalScope? parent = null)
    {
        var body = MakeFunction(ns, f.Name.Name, parent);
        var types = new Dictionary<string, TypeGenericsParameter>();

        ITypeDefinition create_type(ITypeNode s)
        {
            if (s is TypeArrayNode ta)
            {
                var xs_type = CreateType(body, new TypeNode() { Name = $"xs${body.Generics.Count}" }).Cast<TypeGenericsParameter>();
                body.Generics.Add(xs_type);

                var x_type = create_type(ta.Item);
                body.Constraints.Add((new VariableValue() { Name = "List" }, new ITypeDefinition[] { xs_type, x_type }.ToList()));
                return xs_type;
            }
            else if (s is SpecializationNode sp)
            {
                sp.Generics.Each(x => create_type(x));
            }
            return types.TryGetValue(s.Name, out var value) ? value
                : gens?.FirstOrDefault(x => x.Name == s.Name) is { } p ? types[s.Name] = p.Return(x => body.Generics.Add(x))
                : CreateType(body, s).Return(x => { if (x is TypeGenericsParameter g) body.Generics.Add(types[g.Name] = g); });
        }

        f.Arguments.Each(x =>
        {
            var name = new VariableValue() { Name = x.Name.Name };
            body.Arguments.Add((name, create_type(x.Type)));
            body.LexicalScope.Add(x.Name.Name, name);
        });
        if (f.Return is { }) body.Return = create_type(f.Return);

        f.Constraints.Each(x => body.Constraints.Add((new VariableValue() { Name = x.Name }, x.Generics.Select(g => create_type(g)).ToList())));
        f.Functions.Each(x => MakeFunctionBodyDefinition(ns, x, body));

        if (body.Generics.Count == 0) body.SpecializationMapper[[]] = [];
        return body;
    }
}
