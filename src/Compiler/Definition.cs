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
            pgm.Structs.Each(x => src.Structs.Add(TypeBodyDefinition(src, x)));
        }

        public static StructBody TypeBodyDefinition(SourceCodeBody src, StructNode sn)
        {
            var body = new StructBody(src, sn.Name.Name);
            sn.Generics.Each(x => body.Generics.Add(new TypeValue(x.Cast<TypeNode>().Name) { Types = Types.Generics }));
            FunctionBodyDefinition(body, sn.Statements);
            sn.Statements.Each(let =>
            {
                switch (let)
                {
                    case LetNode x:
                        body.Members.Add(x.Var.Name, body.LexicalScope[x.Var.Name]);
                        break;

                    case LetTypeNode x:
                        body.Members.Add(x.Var.Name, body.LexicalScope[x.Var.Name]);
                        break;

                    default:
                        throw new Exception();
                }
            });
            if (sn.Generics.Count == 0)
            {
                body.SpecializationMapper[new GenericsMapper()] = new TypeMapper();
                src.Functions.Add(new EmbeddedFunction(sn.Name.Name, sn.Name.Name) { OpCode = (args) => $"newobj instance void {sn.Name.Name}::.ctor()" });
            }
            return body;
        }

        public static void FunctionDefinition(SourceCodeBody src, ProgramNode pgm)
        {
            if (pgm.Statements.Count > 0)
            {
                var body = MakeFunction(src, "main");
                body.SpecializationMapper[new GenericsMapper()] = new TypeMapper();
                FunctionBodyDefinition(body, pgm.Statements);
            }

            pgm.Functions.Each(f =>
            {
                var body = MakeFunction(src, f.Name.Name);
                var types = new Dictionary<string, TypeValue>();
                TypeValue create_type(string s) => types.ContainsKey(s) ? types[s] : new TypeValue(s).Return(x => types[s] = x).Return(x => { if (x.Types == Types.Generics) body.Generics.Add(x); });

                f.Arguments.Each(x =>
                {
                    var name = new VariableValue(x.Name.Name);
                    body.Arguments.Add((name, create_type(x.Type.Cast<TypeNode>().Name)));
                    body.LexicalScope.Add(x.Name.Name, name);
                });
                if (f.Return is { }) body.Return = create_type(f.Return.Cast<TypeNode>().Name);

                if (body.Generics.Count == 0) body.SpecializationMapper[new GenericsMapper()] = new TypeMapper();
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
                        {
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
                        }
                        break;

                    case LetTypeNode let:
                        {
                            var v = new VariableValue(let.Var.Name);
                            scope.LexicalScope.Add(let.Var.Name, v);
                            scope.Body.Add(new TypeBind(v, new TypeValue(let.Type.Cast<TypeNode>().Name)));
                        }
                        break;

                    case LetPropertyNode let:
                        scope.Body.Add(new Code { Operator = Operator.Bind, Return = new PropertyValue(NormalizationExpression(scope, let.Reciever, true), let.Name.Name), Left = NormalizationExpression(scope, let.Expression, true) });
                        break;

                    case FunctionCallNode call:
                        scope.Body.Add(new Call(NormalizationExpression(scope, call).Cast<FunctionCallValue>()));
                        break;

                    case IIfNode if_:
                        var else_ = new LabelCode() { Name = "Else" };
                        var elseif = if_.ElseIf.Map(x => new LabelCode() { Name = "ElseIf" }).ToArray();
                        var endif = new LabelCode() { Name = "EndIf" };

                        var next_label =
                            elseif.Length > 0 ? elseif.First()
                            : if_.Else is { } ? else_
                            : endif;
                        var inner_scope = new InnerScope(scope);

                        if (if_ is IfNode ifn)
                        {
                            inner_scope.Body.Add(new IfCode(NormalizationExpression(inner_scope, ifn.Condition, true), next_label));
                        }
                        else
                        {
                            var ifc = if_.Cast<IfCastNode>();
                            var ifcast = new IfCastCode(new VariableValue(ifc.Name.Name), new TypeValue(ifc.Declare.Cast<TypeNode>().Name), NormalizationExpression(inner_scope, ifc.Condition, true), next_label);
                            inner_scope.Body.Add(ifcast);
                            inner_scope.LexicalScope.Add(ifc.Name.Name, ifcast.Name);
                        }

                        FunctionBodyDefinition(inner_scope, if_.Then.Statements);
                        scope.Body.AddRange(inner_scope.Body);
                        scope.MaxTemporaryValue = inner_scope.MaxTemporaryValue;

                        if_.ElseIf.Each((x, i) =>
                        {
                            scope.Body.Add(new GotoCode(endif));
                            scope.Body.Add(elseif[i]);

                            var next_label =
                                elseif.Length > i + 1 ? elseif[i + 1]
                                : if_.Else is { } ? else_
                                : endif;
                            var inner_scope = new InnerScope(scope);

                            if (x is IfNode ifn)
                            {
                                inner_scope.Body.Add(new IfCode(NormalizationExpression(inner_scope, ifn.Condition, true), next_label));
                            }
                            else
                            {
                                var ifc = x.Cast<IfCastNode>();
                                var ifcast = new IfCastCode(new VariableValue(ifc.Name.Name), new TypeValue(ifc.Declare.Cast<TypeNode>().Name), NormalizationExpression(inner_scope, ifc.Condition, true), next_label);
                                inner_scope.Body.Add(ifcast);
                                inner_scope.LexicalScope.Add(ifc.Name.Name, ifcast.Name);
                            }

                            FunctionBodyDefinition(inner_scope, x.Then.Statements);
                            scope.Body.AddRange(inner_scope.Body);
                            scope.MaxTemporaryValue = inner_scope.MaxTemporaryValue;
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
                        var call =
                            x.Expression is PropertyNode prop ? new FunctionCallValue(new VariableValue(prop.Right.Name)) { FirstLookup = NormalizationExpression(scope, prop.Left) }
                            : x.Expression is TypeGenericsNode gen ? new FunctionCallValue(CreateTypeGenerics(gen))
                            : new FunctionCallValue(new VariableValue(GetName(x.Expression)));

                        x.Arguments.Each(x => call.Arguments.Add(NormalizationExpression(scope, x, true)));
                        return call;
                    }

                case PropertyNode x:
                    return new PropertyValue(NormalizationExpression(scope, x.Left, true), x.Right.Name);

                case ListNode<IEvaluableNode> x:
                    return new ArrayContainer(x.List.Map(list => NormalizationExpression(scope, list, true)).ToList());
            }
            throw new Exception();
        }

        public static TypeGenericsValue CreateTypeGenerics(TypeGenericsNode gen)
        {
            var g = new TypeGenericsValue(gen.Name);
            gen.Generics.Map(x => x switch
            {
                TypeNode t => (ITypeDefinition)new TypeValue(t.Name),
                TypeGenericsNode t => CreateTypeGenerics(t),
                _ => throw new Exception(),
            }).Each(x => g.Generics.Add(x));
            return g;
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
            var max = ++scope.MaxTemporaryValue;
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
