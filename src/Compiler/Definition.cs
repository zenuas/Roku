using Extensions;
using Roku.Declare;
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
            var src = new SourceCodeBody();
            src.Uses.Add(root);
            TypeDefinition(src, pgm);
            FunctionDefinition(src, pgm);
            ClassDefinition(src, pgm);
            return src;
        }

        public static void TypeDefinition(SourceCodeBody src, ProgramNode pgm)
        {
            pgm.Structs.Each(x => src.Structs.Add(TypeBodyDefinition(src, x)));
        }

        public static StructBody TypeBodyDefinition(SourceCodeBody src, StructNode sn)
        {
            var body = new StructBody(src, sn.Name.Name);
            sn.Generics.Each(x => body.Generics.Add(new TypeGenericsParameter(x.Name)));
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
                src.Functions.Add(new EmbeddedFunction(sn.Name.Name, sn.Name.Name) { OpCode = (_, args) => $"newobj instance void {sn.Name.Name}::.ctor()" });
            }
            return body;
        }

        public static StructBody TupleDefinition(RootNamespace root, int count)
        {
            var name = GetTupleName(count);
            var exists = root.Structs.FindFirstOrNull(x => x.Name == name);

            if (exists is StructBody sb) return sb;

            var body = new StructBody(root, name);
            Lists.RangeTo(1, count).Each(i =>
            {
                var gp = new TypeGenericsParameter($"a{i}");
                body.Generics.Add(gp);
                var member = new VariableValue($"{i}");
                body.LexicalScope.Add(member.Name, member);
                body.Body.Add(new TypeBind(member, gp));
                body.Members.Add(member.Name, member);
            });
            root.Structs.Add(body);
            return body;
        }

        public static FunctionBody TupleBodyDefinition(RootNamespace root, int count)
        {
            var name = GetTupleName(count);
            var exists = root.Structs.FindFirstOrNull(x => x.Name == name);

            if (exists is null) _ = TupleDefinition(root, count);

            var fbody = MakeFunction(root, $"{name}#{root.TupleUniqueCount++}");
            var fret = new TypeSpecialization(new VariableValue(name));
            var fcall = new TypeSpecialization(new VariableValue(name));
            var self = new VariableValue("$self");
            fbody.LexicalScope.Add(self.Name, self);
            fbody.Body.Add(new Call(new FunctionCallValue(fcall)) { Return = self });

            Lists.Range(0, count).Each(i =>
            {
                var gp = new TypeGenericsParameter($"t{i + 1}");
                var farg_var = new VariableValue($"x{i + 1}");
                fbody.Generics.Add(gp);
                fbody.Arguments.Add((farg_var, gp));
                fbody.LexicalScope.Add(farg_var.Name, farg_var);
                fbody.Body.Add(new Code { Operator = Operator.Bind, Return = new PropertyValue(self, $"{i + 1}"), Left = farg_var });
                fret.Generics.Add(gp);
                fcall.Generics.Add(gp);
            });
            fbody.Body.Add(new Call(new FunctionCallValue(new VariableValue("return")).Return(x => x.Arguments.Add(self))));
            fbody.Return = fret;
            return fbody;
        }

        public static AnonymousFunctionBody LambdaExpressionDefinition(ILexicalScope scope, LambdaExpressionNode lambda)
        {
            var fbody = MakeAnonymousFunction(scope.Namespace);
            fbody.IsImplicit = lambda.IsImplicit;
            if (lambda.Return is { } ret) fbody.Return = CreateType(scope, ret);
            if (fbody.IsImplicit) fbody.Return = new TypeImplicit();
            lambda.Arguments.Each(x => fbody.Arguments.Add((new VariableValue(x.Name.Name), x is DeclareNode decla ? CreateType(scope, decla.Type) : new TypeImplicit())));

            if (fbody.Generics.Count == 0) fbody.SpecializationMapper[new GenericsMapper()] = new TypeMapper();
            FunctionBodyDefinition(fbody, lambda.Statements);
            return fbody;
        }

        public static void FunctionDefinition(SourceCodeBody src, IScopeNode scope)
        {
            if (scope.Statements.Count > 0)
            {
                var body = MakeFunction(src, "main");
                body.SpecializationMapper[new GenericsMapper()] = new TypeMapper();
                FunctionBodyDefinition(body, scope.Statements);
            }

            scope.Functions.Each(f =>
            {
                var body = MakeFunctionDefinition(src, null, f);
                FunctionBodyDefinition(body, f.Statements);

                if (body.Body.Contains(IsYield)) ConvertCoroutine(src, scope, body);
            });
        }

        public static ITypeDefinition CreateType(ILexicalScope scope, ITypeNode t)
        {
            switch (t)
            {
                case EnumNode en:
                    return new TypeEnum(en.Types.Map(x => CreateType(scope, x)));

                case TypeNode tn:
                    if (!char.IsLower(tn.Name.First())) return new TypeValue(tn.Name);
                    if (scope.LexicalScope.ContainsKey(tn.Name)) return scope.LexicalScope[tn.Name].Cast<ITypeDefinition>();
                    var gen = new TypeGenericsParameter(tn.Name);
                    scope.LexicalScope[tn.Name] = gen;
                    return gen;

                case SpecializationNode sp:
                    return CreateTypeSpecialization(scope, sp);

                case TypeTupleNode tp:
                    return CreateTupleSpecialization(scope, tp);

                case TypeStructNode ts:
                    return CreateTypeStruct(scope, ts);

                case TypeFunctionNode tf:
                    return CreateTypeFunction(scope, tf);

                default:
                    throw new Exception();
            }
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
                            scope.Body.Add(new TypeBind(v, CreateType(scope, let.Type)));
                        }
                        break;

                    case LetPropertyNode let:
                        scope.Body.Add(new Code { Operator = Operator.Bind, Return = new PropertyValue(NormalizationExpression(scope, let.Reciever, true), let.Name.Name), Left = NormalizationExpression(scope, let.Expression, true) });
                        break;

                    case LetTupleAssignmentNode let:
                        {
                            var e = NormalizationExpression(scope, let.Expression, true);
                            let.Assignment.Each((x, i) =>
                            {
                                if (x is LetVarNode letv)
                                {
                                    var v = new VariableValue(letv.Var.Name);
                                    scope.LexicalScope.Add(letv.Var.Name, v);
                                    scope.Body.Add(new Code { Operator = Operator.Bind, Return = v, Left = new PropertyValue(e, $"{i + 1}") });
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
                                scope.Body.Add(new Code { Operator = Operator.Bind, Return = v, Left = e });
                            }
                        }
                        break;

                    default:
                        throw new Exception();
                }
            });
        }

        public static void IfBodyDefinition(ILexicalScope scope, IIfNode if_)
        {
            var else_ = new LabelCode() { Name = "Else" };
            var elseif = if_.ElseIf.Map(x => new LabelCode() { Name = "ElseIf" }).ToArray();
            var endif = new LabelCode() { Name = "EndIf" };

            var next_label =
                elseif.Length > 0 ? elseif.First()
                : if_.Else is { } ? else_
                : endif;
            IfThenDefinition(scope, if_, next_label);

            if_.ElseIf.Each((x, i) =>
            {
                scope.Body.Add(new GotoCode(endif));
                scope.Body.Add(elseif[i]);

                var next_label =
                    elseif.Length > i + 1 ? elseif[i + 1]
                    : if_.Else is { } ? else_
                    : endif;

                IfThenDefinition(scope, x, next_label);
            });

            if (if_.Else is { })
            {
                scope.Body.Add(new GotoCode(endif));
                scope.Body.Add(else_);
                FunctionBodyDefinition(scope, if_.Else.Statements);
            }
            scope.Body.Add(endif);
        }

        private static void IfThenDefinition(ILexicalScope scope, IIfNode if_, LabelCode next_label)
        {
            var inner_scope = new InnerScope(scope);

            if (if_ is IfNode ifn)
            {
                inner_scope.Body.Add(new IfCode(NormalizationExpression(inner_scope, ifn.Condition, true), next_label));
            }
            else
            {
                var ifc = if_.Cast<IfCastNode>();
                var ifcast = new IfCastCode(new VariableValue(ifc.Name.Name), CreateType(inner_scope, ifc.Declare), NormalizationExpression(inner_scope, ifc.Condition, true), next_label);
                inner_scope.Body.Add(ifcast);
                inner_scope.LexicalScope.Add(ifc.Name.Name, ifcast.Name);
            }

            FunctionBodyDefinition(inner_scope, if_.Then.Statements);
            scope.Body.AddRange(inner_scope.Body);
            scope.MaxTemporaryValue = inner_scope.MaxTemporaryValue;
        }

        public static IEvaluable NormalizationExpression(ILexicalScope scope, IEvaluableNode e, bool evaluate_as_expression = false)
        {
            switch (e)
            {
                case StringNode x:
                    return new StringValue(x.Value);

                case NumericNode x:
                    return new NumericValue(x.Value);

                case FloatingNumericNode x:
                    return new FloatingNumericValue(x.Value);

                case BooleanNode x:
                    return new BooleanValue(x.Value);

                case NullNode x:
                    return new NullValue();

                case VariableNode x:
                    return FindScopeValue(scope, x.Name);

                case TypeNode x:
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
                            : x.Expression is SpecializationNode gen ? new FunctionCallValue(CreateTypeSpecialization(scope, gen))
                            : x.Expression is VariableNode va && FindCurrentScopeValueOrNull(scope, va.Name) is { } v ? new FunctionCallValue(v)
                            : new FunctionCallValue(new VariableValue(GetName(x.Expression)));

                        x.Arguments.Each(x => call.Arguments.Add(NormalizationExpression(scope, x, true)));
                        return call;
                    }

                case PropertyNode x:
                    {
                        var prop = new PropertyValue(NormalizationExpression(scope, x.Left, true), x.Right.Name);
                        if (!evaluate_as_expression) return prop;
                        var v = CreateTemporaryVariable(scope);
                        scope.Body.Add(new Code() { Operator = Operator.Bind, Left = prop, Return = v });
                        return v;
                    }

                case ListNode<IEvaluableNode> x:
                    return new ArrayContainer(x.List.Map(list => NormalizationExpression(scope, list, true)).ToList());

                case TupleNode x:
                    {
                        var f = TupleBodyDefinition(Lookup.GetRootNamespace(scope.Namespace), x.Values.Count);
                        var call = new FunctionCallValue(new VariableValue(f.Name));
                        x.Values.Each(x => call.Arguments.Add(NormalizationExpression(scope, x, true)));
                        if (!evaluate_as_expression) return call;
                        var v = CreateTemporaryVariable(scope);
                        scope.Body.Add(new Call(call) { Return = v });
                        return v;
                    }

                case LambdaExpressionNode x:
                    {
                        var f = LambdaExpressionDefinition(scope, x);
                        var v = CreateTemporaryVariable(scope);
                        scope.Body.Add(new Code() { Operator = Operator.Bind, Left = new FunctionReferenceValue(f.Name), Return = v });
                        return v;
                    }
            }
            throw new Exception();
        }

        public static void ConvertCoroutine(SourceCodeBody src, IScopeNode scope, FunctionBody body)
        {
            /*
                struct Co$0
                    var state = 0
                    var value: a
                    var next: [Co$0 | Null]
            */
            var list_a = body.Constraints.FindFirst(x => x.Class.Name == "List" && x.Generics.Count == 2 && x.Generics[0] == body.Return).Generics[1];
            var co_struct = new StructBody(src, $"Co${src.CoroutineUniqueCount++}");
            var co_struct_typename = new TypeValue(co_struct.Name);
            var state = new VariableValue("state");
            var value = new VariableValue("value");
            var next = new VariableValue("next");
            co_struct.Members.Add("state", co_struct.LexicalScope["state"] = state);
            co_struct.Members.Add("value", co_struct.LexicalScope["value"] = value);
            co_struct.Members.Add("next", co_struct.LexicalScope["next"] = next);
            co_struct.Body.Add(new Code { Operator = Operator.Bind, Return = state, Left = new NumericValue(0) });
            co_struct.Body.Add(new TypeBind(value, list_a));
            co_struct.Body.Add(new TypeBind(next, new TypeEnum(new ITypeDefinition[] { co_struct_typename, new TypeValue("Null") })));
            src.Structs.Add(co_struct);

            var tuple2 = TupleBodyDefinition(Lookup.GetRootNamespace(src), 2);

            // sub next($self: Co$0) [a, Co$0]
            var next_body = new FunctionBody(src, "next");
            var self = new VariableValue("$self");
            next_body.Arguments.Add((self, co_struct_typename));
            next_body.LexicalScope["$self"] = self;
            var tuple2sp = new TypeSpecialization(new VariableValue(GetTupleName(2)));
            tuple2sp.Generics.Add(list_a);
            tuple2sp.Generics.Add(co_struct_typename);
            next_body.Return = tuple2sp;
            next_body.Body.AddRange(body.Body);
            src.Functions.Add(next_body);

            /*
                sub co<List<x, a>>() x
                    var $ret = Co$0()
                    return($ret)
            */
            var ret = new VariableValue("$ret");
            body.Body.Clear();
            next_body.LexicalScope["$ret"] = ret;
            body.Body.Add(new Call(new FunctionCallValue(new VariableValue(co_struct.Name))) { Return = ret });
            body.Body.Add(new Call(new FunctionCallValue(new VariableValue("return")).Return(x => x.Arguments.Add(ret))));

            /*
                sub isnull($self: Co$0) Bool
                    next($self)
                    var m1 = - 1
                    var $ret = $self.state == m1
                    return($ret)
            */
            var isnull_body = new FunctionBody(src, "isnull");
            isnull_body.Arguments.Add((self, co_struct_typename));
            next_body.Return = new TypeValue("Bool");
            var m1 = new VariableValue("m1");
            isnull_body.LexicalScope["$self"] = self;
            isnull_body.LexicalScope["m1"] = m1;
            isnull_body.LexicalScope["$ret"] = ret;
            isnull_body.Body.Add(new Call(new FunctionCallValue(next).Return(x => x.Arguments.Add(self))));
            isnull_body.Body.Add(new Call(new FunctionCallValue(new VariableValue("-")).Return(x => x.Arguments.Add(new NumericValue(1)))) { Return = m1 });
            isnull_body.Body.Add(new Call(new FunctionCallValue(new VariableValue("==")).Return(x => x.Arguments.AddRange(new IEvaluable[] { new PropertyValue(self, "state"), m1 }))) { Return = ret });
            isnull_body.Body.Add(new Call(new FunctionCallValue(new VariableValue("return")).Return(x => x.Arguments.Add(ret))));
            src.Functions.Add(isnull_body);
        }

        public static bool IsYield(IOperand x) => x is Call call && call.Function.Function is VariableValue name && name.Name == "yield";

        public static TypeSpecialization CreateTypeSpecialization(ILexicalScope scope, SpecializationNode gen)
        {
            var g = new TypeSpecialization(NormalizationExpression(scope, gen.Expression));
            gen.Generics.Map(x => x switch
            {
                TypeNode t => CreateType(scope, t),
                SpecializationNode t => CreateTypeSpecialization(scope, t),
                _ => throw new Exception(),
            }).Each(x => g.Generics.Add(x));
            return g;
        }

        public static TypeSpecialization CreateTupleSpecialization(ILexicalScope scope, TypeTupleNode tuple)
        {
            var g = new TypeSpecialization(new VariableValue(GetName(tuple)));
            tuple.Types.Map(x => x switch
            {
                TypeNode t => CreateType(scope, t),
                SpecializationNode t => CreateTypeSpecialization(scope, t),
                _ => throw new Exception(),
            }).Each(x => g.Generics.Add(x));
            return g;
        }

        public static TypeValue CreateTypeStruct(ILexicalScope scope, TypeStructNode st)
        {
            var top = Lookup.GetTopLevelNamespace(scope.Namespace);
            var name = GetName(st);
            var exists = top.Structs.FindFirstOrNull(x => x.Name == name);

            if (exists is null)
            {
                var args = st.Arguments.Map(x => (x.Name.Name, Type: CreateType(scope, x.Type)));

                var body = new StructBody(top, name);
                top.Structs.Add(body);

                var ctor = MakeFunction(top, st.StructName.Name);
                var self = new VariableValue("$self");
                ctor.LexicalScope.Add(self.Name, self);
                ctor.Body.Add(new Call(new FunctionCallValue(new VariableValue(name))) { Return = self });
                top.Functions.Add(new EmbeddedFunction(name, name) { OpCode = (_, args) => $"newobj instance void {CodeGenerator.EscapeILName(name)}::.ctor()" });
                args.Each((x, i) =>
                {
                    var member = new VariableValue(x.Name);
                    body.LexicalScope.Add(member.Name, member);
                    body.Body.Add(new TypeBind(member, x.Type));
                    body.Members.Add(member.Name, member);

                    var farg_var = new VariableValue(x.Name);
                    ctor.Arguments.Add((farg_var, x.Type));
                    ctor.LexicalScope.Add(farg_var.Name, farg_var);
                    ctor.Body.Add(new Code { Operator = Operator.Bind, Return = new PropertyValue(self, farg_var.Name), Left = farg_var });
                });
                ctor.Body.Add(new Call(new FunctionCallValue(new VariableValue("return")).Return(x => x.Arguments.Add(self))));
                ctor.Return = new TypeValue(name);
            }

            return new TypeValue(name);
        }

        public static TypeFunction CreateTypeFunction(ILexicalScope scope, TypeFunctionNode tf)
        {
            var func = new TypeFunction();
            tf.Arguments.Each(x => func.Arguments.Add(CreateType(scope, x)));
            if (tf.Return is { } r) func.Return = CreateType(scope, r);
            return func;
        }

        public static string GetName(IEvaluableNode node)
        {
            return node switch
            {
                VariableNode x => x.Name,
                TokenNode x => x.Token.Name,
                TupleNode x => GetTupleName(x.Values.Count),
                TypeTupleNode x => GetTupleName(x.Types.Count),
                TypeStructNode x => x.Name,
                _ => throw new Exception(),
            };
        }

        public static string GetTupleName(int count) => $"Tuple#{count}";

        public static IEvaluable CreateTemporaryVariable(ILexicalScope scope)
        {
            var max = ++scope.MaxTemporaryValue;
            var v = new TemporaryValue($"$${max}", max, scope);
            scope.LexicalScope.Add(v.Name, v);
            return v;
        }
        public static IEvaluable? FindCurrentScopeValueOrNull(ILexicalScope scope, string name) => scope.LexicalScope.ContainsKey(name) ? scope.LexicalScope[name] : null;

        public static IEvaluable FindScopeValue(ILexicalScope scope, string name)
        {
            if (FindCurrentScopeValueOrNull(scope, name) is { } v) return v;
            if (scope.LexicalScope.ContainsKey(name)) return scope.LexicalScope[name];
            if (scope.Parent is { } parent) return FindScopeValue(parent, name);
            if (scope is INamespace ns && FindNamespaceValue(ns, name) is { } p) return p;
            if (scope.Namespace is IUse src)
            {
                return src.Uses.Map(x => FindNamespaceValue(x, name)).By<IEvaluable>().First();
            }
            throw new Exception();
        }

        public static IEvaluable? FindNamespaceValue(INamespace ns, string name)
        {
            if (ns is INamespaceBody body && body.Structs.FindFirstOrNull(x => x.Name == name) is { } s) return new TypeValue(s.Name);
            if (ns is RootNamespace root) return new TypeValue(name);
            return null;
        }

        public static FunctionBody MakeFunction(INamespaceBody ns, string name)
        {
            var body = new FunctionBody(ns, name);
            ns.Functions.Add(body);
            return body;
        }

        public static FunctionBody MakeFunctionDefinition(INamespaceBody ns, List<TypeGenericsParameter>? gens, FunctionNode f)
        {
            var body = MakeFunction(ns, f.Name.Name);
            var types = new Dictionary<string, TypeGenericsParameter>();

            ITypeDefinition create_type(ITypeNode s)
            {
                if (s is TypeArrayNode ta)
                {
                    var xs_type = CreateType(body, new TypeNode() { Name = $"xs${body.Generics.Count}" }).Cast<TypeGenericsParameter>();
                    body.Generics.Add(xs_type);

                    var x_type = create_type(ta.Item);
                    body.Constraints.Add((new VariableValue("List"), new ITypeDefinition[] { xs_type, x_type }.ToList()));
                    return xs_type;
                }
                else
                {
                    return types.ContainsKey(s.Name) ? types[s.Name]
                        : gens?.FindFirstOrNull(x => x.Name == s.Name) is { } p ? types[s.Name] = p.Return(x => body.Generics.Add(x))
                        : CreateType(body, s).Return(x => { if (x is TypeGenericsParameter g) body.Generics.Add(types[g.Name] = g); });
                }
            }

            f.Arguments.Each(x =>
            {
                var name = new VariableValue(x.Name.Name);
                body.Arguments.Add((name, create_type(x.Type)));
                body.LexicalScope.Add(x.Name.Name, name);
            });
            if (f.Return is { }) body.Return = create_type(f.Return);

            f.Constraints.Each(x => body.Constraints.Add((new VariableValue(x.Name), x.Generics.Map(g => create_type(g)).ToList())));

            if (body.Generics.Count == 0) body.SpecializationMapper[new GenericsMapper()] = new TypeMapper();
            return body;
        }

        public static AnonymousFunctionBody MakeAnonymousFunction(INamespace ns)
        {
            var root = Lookup.GetRootNamespace(ns);
            var body = new AnonymousFunctionBody(ns, $"anonymous#{root.AnonymousFunctionUniqueCount++}");
            root.Structs.Add(body);
            root.Functions.Add(body);
            return body;
        }

        public static void ClassDefinition(SourceCodeBody src, ProgramNode pgm)
        {
            pgm.Classes.Each(x => src.Classes.Add(ClassBodyDefinition(src, x)));
        }

        public static ClassBody ClassBodyDefinition(SourceCodeBody src, ClassNode cn)
        {
            var body = new ClassBody(src, cn.Name.Name);
            cn.Generics.Each(x => body.Generics.Add(new TypeGenericsParameter(x.Name)));
            cn.Functions.Each(x => MakeFunctionDefinition(body, body.Generics, x));
            return body;
        }
    }
}
