using Extensions;
using Roku.IntermediateCode;
using Roku.Manager;
using Roku.Node;
using System;
using System.Collections.Generic;

namespace Roku.Compiler
{
    public static class Definition
    {
        public static SourceCodeBody LoadProgram(RootNamespace root, ProgramNode pgm)
        {
            var src = new SourceCodeBody(root);
            src.Uses.Add(root);
            TypeDefinition(src, pgm);
            FunctionDefinition(src, pgm);
            return src;
        }

        public static void TypeDefinition(SourceCodeBody src, ProgramNode pgm)
        {
        }

        public static void FunctionDefinition(SourceCodeBody src, ProgramNode pgm)
        {
            if (pgm.Statements.Count > 0)
            {
                FunctionBodyDefinition(MakeFunction(src, "main"), pgm.Statements);
            }

            var types = new Dictionary<string, TypeValue>();
            TypeValue create_type(string s) => types.ContainsKey(s) ? types[s] : new TypeValue(s) { Types = char.IsLower(s.First()) ? Types.Generics : Types.Struct }.Return(x => types[s] = x);

            pgm.Functions.Each(f =>
                {
                    var body = MakeFunction(src, f.Name.Name);
                    if (f.Return is { }) body.Return = create_type(f.Return.Name);
                    f.Arguments.Each(x =>
                        {
                            //var t = Lookup.LoadStruct(src, x.Type.Name);
                            var name = new VariableValue(x.Name.Name);
                            body.Arguments.Add((name, create_type(x.Type.Name)));
                            body.LexicalScope.Add(x.Name.Name, name);
                        });

                    FunctionBodyDefinition(body, f.Statements);
                });
        }

        public static void FunctionBodyDefinition(ILexicalScope scope, List<IStatementNode> stmts)
        {
            stmts.Each(stmt =>
            {
                switch (stmt)
                {
                    case LetNode let:
                        var v = new VariableValue(let.Var.Name);
                        scope.LexicalScope.Add(let.Var.Name, v);
                        var e = NormalizationExpression(scope, let.Expression);
                        if (e is FunctionCallValue fcall)
                        {
                            scope.Body.Add(new Call(fcall) { Return = v });
                        }
                        else
                        {
                            scope.Body.Add(new Code { Operator = Operator.Bind, Return = v, Left = e });
                        }
                        break;

                    case FunctionCallNode call:
                        scope.Body.Add(new Call(NormalizationExpression(scope, call).Cast<FunctionCallValue>()));
                        break;

                    case IfNode if_:
                        var else_ = new LabelCode() { Name = "Else" };
                        var elseif = if_.ElseIf.Map(x => new LabelCode() { Name = "ElseIf" }).ToArray();
                        var endif = new LabelCode() { Name = "EndIf" };

                        scope.Body.Add(new IfCode(NormalizationExpression(scope, if_.Condition, true),
                            elseif.Length > 0 ? elseif.First()
                            : if_.Else is { } ? else_
                            : endif));

                        FunctionBodyDefinition(scope, if_.Then.Statements);

                        if_.ElseIf.Each((x, i) =>
                        {
                            scope.Body.Add(new GotoCode(endif));

                            scope.Body.Add(elseif[i]);
                            scope.Body.Add(new IfCode(NormalizationExpression(scope, x.Condition, true),
                                elseif.Length > i + 1 ? elseif[i + 1]
                                : if_.Else is { } ? else_
                                : endif));
                            FunctionBodyDefinition(scope, x.Then.Statements);
                        });

                        if (if_.Else is { })
                        {
                            scope.Body.Add(new GotoCode(endif));
                            scope.Body.Add(else_);
                            FunctionBodyDefinition(scope, if_.Else.Statements);
                        }
                        scope.Body.Add(endif);
                        break;

                    default:
                        throw new Exception();
                }
            });
        }

        public static ITypedValue NormalizationExpression(ILexicalScope scope, IEvaluableNode e, bool evaluate_as_expression = false)
        {
            switch (e)
            {
                case StringNode x:
                    return new StringValue(x.Value);

                case NumericNode x:
                    return new NumericValue(x.Value);

                case VariableNode x:
                    return FindScopeValue(scope, x.Name);

                case FunctionCallNode x:
                    if (evaluate_as_expression)
                    {
                        var v = CreateTemporaryVariable(scope);
                        scope.Body.Add(new Call(NormalizationExpression(scope, x).Cast<FunctionCallValue>()) { Return = v });
                        return v;
                    }
                    else
                    {
                        var call = x.Expression is PropertyNode prop
                            ? new FunctionCallValue(new VariableValue(prop.Right.Name)) { FirstLookup = NormalizationExpression(scope, prop.Left) }
                            : new FunctionCallValue(new VariableValue(GetName(x.Expression)));

                        x.Arguments.Each(x => call.Arguments.Add(NormalizationExpression(scope, x, true)));
                        return call;
                    }
            }
            throw new Exception();
        }

        public static string GetName(IEvaluableNode node)
        {
            return node switch
            {
                VariableNode x => x.Name,
                TokenNode x => x.Token.Name,
                _ => throw new Exception(),
            };
        }

        public static ITypedValue CreateTemporaryVariable(ILexicalScope scope)
        {
            var max = scope.LexicalScope.Values.By<TemporaryValue>().FoldLeft((r, x) => Math.Max(r, x.Index + 1), 1);
            var v = new TemporaryValue($"$${max}", max, scope);
            scope.LexicalScope.Add(v.Name, v);
            return v;
        }

        public static ITypedValue FindScopeValue(ILexicalScope scope, string name)
        {
            return scope.LexicalScope.ContainsKey(name) ? scope.LexicalScope[name] : FindScopeValue(scope.Parent!, name);
        }

        public static FunctionBody MakeFunction(INamespace ns, string name)
        {
            var body = new FunctionBody(ns, name);
            ns.Functions.Add(body);
            return body;
        }
    }
}
