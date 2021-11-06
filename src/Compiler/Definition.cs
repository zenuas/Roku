using Extensions;
using Roku.Declare;
using Roku.IntermediateCode;
using Roku.Manager;
using Roku.Node;
using System;
using System.Linq;

namespace Roku.Compiler
{
    public static partial class Definition
    {
        public static SourceCodeBody LoadProgram(RootNamespace root, ProgramNode pgm)
        {
            var src = new SourceCodeBody();
            src.Uses.Add(root);
            TypeDefinition(src, pgm);
            FunctionDefinition(src, pgm);
            ClassDefinition(src, pgm);
            InstanceDefinition(src, pgm);
            return src;
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
                    if (x.Namespace.Count > 0)
                    {
                        return new TypeValue(x.Name).Return(t => t.Namespace.AddRange(x.Namespace));
                    }
                    else
                    {
                        return FindScopeValue(scope, x.Name);
                    }

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
                            x.Expression is PropertyNode prop ? new FunctionCallValue(new VariableValue(prop.Right.Name)) { FirstLookup = NormalizationExpression(scope, prop.Left, true) }
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
                    return new ArrayContainer(x.List.Select(list => NormalizationExpression(scope, list, true)).ToList());

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
                        var fref = new FunctionReferenceValue(f.Name);
                        if (!evaluate_as_expression) return fref;
                        var v = CreateTemporaryVariable(scope);
                        scope.Body.Add(new Code() { Operator = Operator.Bind, Left = fref, Return = v });
                        return v;
                    }
            }
            throw new Exception();
        }

        public static ITypeDefinition CreateType(ILexicalScope scope, ITypeNode t)
        {
            switch (t)
            {
                case EnumNode en:
                    return new TypeEnum(en.Types.Select(x => CreateType(scope, x)));

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

        public static TypeSpecialization CreateTypeSpecialization(ILexicalScope scope, SpecializationNode gen)
        {
            var g = new TypeSpecialization(NormalizationExpression(scope, gen.Expression));
            gen.Generics.Select(x => x switch
            {
                TypeNode t => CreateType(scope, t),
                SpecializationNode t => CreateTypeSpecialization(scope, t),
                _ => throw new Exception(),
            }).Each(x => g.Generics.Add(x));
            return g;
        }

        public static TypeSpecialization CreateTupleSpecialization(ILexicalScope scope, TypeTupleNode tuple)
        {
            _ = TupleBodyDefinition(Lookup.GetRootNamespace(scope.Namespace), tuple.Types.Count);
            var g = new TypeSpecialization(new VariableValue(GetName(tuple)));
            tuple.Types.Select(x => x switch
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
                var args = st.Arguments.Select(x => (x.Name.Name, Type: CreateType(scope, x.Type)));

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
                body.SpecializationMapper[new GenericsMapper()] = new TypeMapper();
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
                return src.Uses.Select(x => FindNamespaceValue(x, name)).OfType<IEvaluable>().First();
            }
            throw new Exception();
        }

        public static IEvaluable? FindNamespaceValue(INamespace ns, string name)
        {
            if (ns is INamespaceBody body && body.Structs.FindFirstOrNull(x => x.Name == name) is { } s) return new TypeValue(s.Name);
            if (ns is RootNamespace root) return new TypeValue(name);
            return null;
        }
    }
}
