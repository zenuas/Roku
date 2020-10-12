using Extensions;
using Roku.Declare;
using Roku.IntermediateCode;
using Roku.Manager;
using System;
using System.Collections.Generic;

namespace Roku.Compiler
{
    public static class Typing
    {
        public static void TypeInference(RootNamespace root, SourceCodeBody src)
        {
            var srcs = Lookup.AllPrograms(src).ToList();

            while (true)
            {
                var resolved = false;

                root.Structs.By<StructBody>().Each(x => resolved = TypeInference(x) || resolved);
                Lookup.AllStructBodies(srcs).Each(x => resolved = TypeInference(x) || resolved);
                Lookup.AllFunctionBodies(srcs).Concat(Lookup.AllFunctionBodies(root)).Each(x => resolved = TypeInference(x) || resolved);

                if (!resolved) break;
            }
        }

        public static bool TypeInference(StructBody body)
        {
            var resolved = StructBodyInference(body);
            SpecializationNumericDecide(body);
            _ = StructBodyInference(body);

            VariableValue self;
            if (body.LexicalScope.ContainsKey("$self"))
            {
                self = body.LexicalScope["$self"].Cast<VariableValue>();
            }
            else
            {
                self = new VariableValue("$self");
                body.LexicalScope.Add(self.Name, self);
            }

            foreach (var mapper in body.SpecializationMapper.Values)
            {
                if (!mapper.ContainsKey(self)) mapper[self] = CreateVariableDetail(self.Name, body, VariableType.Argument, 0);

                body.Members.Values.Each(x =>
                {
                    var d = mapper[x];
                    d.Type = VariableType.Property;
                    d.Reciever = self;
                    d.Index = 0;
                });
            }
            return resolved;
        }

        public static bool StructBodyInference(StructBody body)
        {
            var resolved = false;
            var keys = body.SpecializationMapper.Keys.ToArray();
            for (var i = 0; i < keys.Length; i++)
            {
                var value = body.SpecializationMapper[keys[i]];
                body.Body.Each(x => resolved = OperandTypeInference(body.Namespace, value, x) || resolved);
            }
            return resolved;
        }

        public static bool TypeInference(FunctionBody body)
        {
            var resolved = FunctionBodyInference(body);
            SpecializationNumericDecide(body);
            _ = FunctionBodyInference(body);

            foreach (var mapper in body.SpecializationMapper.Values)
            {
                if (body.Body.By<IfCastCode>().FindFirstOrNull(x => Lookup.IsValueType(mapper[x.Condition].Struct)) is { })
                {
                    LocalValueInferenceWithEffect(body.Namespace, mapper, mapper.CastBoxCondition, Lookup.LoadType(Lookup.GetRootNamespace(body.Namespace), typeof(object)));
                }
            }
            return resolved;
        }

        public static bool FunctionBodyInference(FunctionBody body)
        {
            var resolved = false;
            var keys = body.SpecializationMapper.Keys.ToArray();
            for (var i = 0; i < keys.Length; i++)
            {
                var value = body.SpecializationMapper[keys[i]];
                body.Arguments.Each((x, i) => resolved = ArgumentInferenceWithEffect(body.Namespace, value, x.Name, x.Type, i) || resolved);
                if (body.Return is { } x) resolved = TypeInferenceWithEffect(body.Namespace, value, x, x) || resolved;
                body.Body.Each(x => resolved = OperandTypeInference(body, value, x) || resolved);
            }
            return resolved;
        }

        public static void SpecializationNumericDecide(ISpecialization sp) => sp.SpecializationMapper.Values.Each(TypeMapperNumericDecide);

        public static void TypeMapperNumericDecide(TypeMapper m)
        {
            var nums = m.Where(x => x.Value.Struct is NumericStruct).ToList();
            for (var i = 0; i < nums.Count; i++)
            {
                nums[i].Value.Struct = nums[i].Value.Struct!.Cast<NumericStruct>().Types.First();
            }
            foreach (var fm in m.Where(x => x.Value.Struct is FunctionMapper).Map(x => x.Value.Struct!.Cast<FunctionMapper>()))
            {
                if (fm.Function is ISpecialization sp) SpecializationNumericDecide(sp);
                TypeMapperNumericDecide(fm.TypeMapper);
            }
        }

        public static bool OperandTypeInference(INamespace ns, TypeMapper m, IOperand op)
        {
            switch (op)
            {
                case Code x when x.Operator == Operator.Bind:
                    var resolve = false;
                    if (x.Return is PropertyValue prop)
                    {
                        resolve = LocalValueInferenceWithEffect(ns, m, prop, ToTypedValue(ns, m, prop).Struct);
                    }
                    return LocalValueInferenceWithEffect(ns, m, x.Return!, ToTypedValue(ns, m, x.Left!).Struct) || resolve;

                case Call x:
                    return ResolveFunctionWithEffect(ns, m, x);

                case TypeBind x:
                    if (x.Type is TypeGenericsParameter g)
                    {
                        return LocalValueInferenceWithEffect(ns, m, x.Name, FindTypeMapperToGenerics(m, x.Type.Name));
                    }
                    else
                    {
                        return LocalValueInferenceWithEffect(ns, m, x.Name, Lookup.LoadStruct(ns, x.Type.Name));
                    }

                case IfCastCode x:
                    return LocalValueInferenceWithEffect(ns, m, x.Name, Lookup.LoadStruct(ns, x.Type.Name));

                case IfCode _:
                case GotoCode _:
                case LabelCode _:
                    return false;
            }
            throw new Exception();
        }

        public static IStructBody FindTypeMapperToGenerics(TypeMapper m, string name) => m.Where(x => x.Key is ITypeDefinition t && t.Name == name).First().Value.Struct!;

        public static bool ResolveFunctionWithEffect(INamespace ns, TypeMapper m, Call call)
        {
            if (m.ContainsKey(call.Function.Function) && m[call.Function.Function].Struct is { } p && IsDecideType(p)) return false;

            var resolve = false;
            var lookupns = ns;
            if (call.Function.FirstLookup is { } receiver)
            {
                if (m.ContainsKey(receiver) && m[receiver] is { } r)
                {
                    if (r.Struct is { }) lookupns = GetStructNamespace(ns, r.Struct);
                    if (r.Type == VariableType.LocalVariable && !call.Function.ReceiverToArgumentsInserted)
                    {
                        call.Function.Arguments.Insert(0, receiver);
                        call.Function.ReceiverToArgumentsInserted = true;
                    }
                }
            }
            var args = call.Function.Arguments.Map(x => ToTypedValue(ns, m, x).Struct).ToList();

            switch (call.Function.Function)
            {
                case VariableValue x when call.Return is null && call.Function.FirstLookup is null && x.Name == "return":
                    {
                        //var ret = new EmbeddedFunction("return", null, args.Map(x => x?.Name!).ToArray()) { OpCode = (args) => $"{(args.Length == 0 ? "" : args[0] + "\n")}ret" };
                        var r = ns.Cast<FunctionBody>().Return;
                        var ret = new EmbeddedFunction("return", null, r is { } ? new ITypeDefinition[] { r } : new ITypeDefinition[] { }) { OpCode = (args) => $"{(args.Length == 0 ? "" : args[0] + "\n")}ret" };
                        var fm = new FunctionMapper(ret);
                        if (r is TypeGenericsParameter gen) fm.TypeMapper[gen] = CreateVariableDetail("", m[gen].Struct, VariableType.TypeParameter); ;
                        m[x] = CreateVariableDetail("", fm, VariableType.FunctionMapper);
                        return true;
                    }

                case VariableValue x:
                    {
                        var caller = Lookup.FindFunctionOrNull(lookupns, x.Name, args);
                        if (caller is null) break;

                        var fm = new FunctionMapper(caller.Body);
                        caller.GenericsMapper.Each(p => fm.TypeMapper[p.Key] = CreateVariableDetail(p.Key.Name, p.Value, VariableType.TypeParameter));
                        IStructBody? ret = null;
                        if (caller.Body is FunctionBody fb)
                        {
                            if (fb.Return is { } && !fm.TypeMapper.ContainsKey(fb.Return)) fm.TypeMapper[fb.Return] = CreateVariableDetail("", Lookup.GetArgumentType(fb.Namespace, fb.Return, caller.GenericsMapper), VariableType.Type);
                            if (fb.Return is { }) ret = fm.TypeMapper[fb.Return].Struct;
                            fb.Arguments.Each((x, i) => fm.TypeMapper[x.Name] = CreateVariableDetail(x.Name.Name, Lookup.GetArgumentType(fb.Namespace, x.Type, caller.GenericsMapper), VariableType.Argument, i));
                            fb.Arguments.Each((x, i) => Feedback(args[i], fm.TypeMapper[x.Name].Struct));
                        }
                        else if (caller.Body is ExternFunction fx)
                        {
                            ret = Lookup.LoadTypeWithoutVoid(Lookup.GetRootNamespace(ns), fx.Function.ReturnType, caller.GenericsMapper);
                        }
                        else if (caller.Body is EmbeddedFunction ef)
                        {
                            if (ef.Return is { } && !fm.TypeMapper.ContainsKey(ef.Return)) fm.TypeMapper[ef.Return] = CreateVariableDetail("", Lookup.LoadStruct(ns, ef.Return.Name), VariableType.Type);
                            if (ef.Return is { }) ret = fm.TypeMapper[ef.Return].Struct;
                            ef.Arguments.Each((x, i) => fm.TypeMapper[x] = CreateVariableDetail($"${i}", Lookup.LoadStruct(ns, x.Name), VariableType.Argument, i));
                        }
                        m[x] = CreateVariableDetail("", fm, VariableType.FunctionMapper);
                        if (call.Return is { }) LocalValueInferenceWithEffect(ns, m, call.Return!, ret);
                        resolve = true;
                    }
                    break;

                case TypeGenericsValue x:
                    {
                        var gens = x.Generics.Map(x => Lookup.GetStructType(ns, x, m)!).ToList();
                        var body = Lookup.FindStructOrNull(ns, GetStructNames(m, x).ToArray(), gens);
                        if (body is null) break;

                        var fm = new FunctionMapper(new EmbeddedFunction(x.ToString(), x.ToString()) { OpCode = (args) => $"newobj instance void {CodeGenerator.GetStructName(body)}::.ctor()" });
                        m[x] = CreateVariableDetail("", fm, VariableType.FunctionMapper);
                        if (call.Return is { }) LocalValueInferenceWithEffect(ns, m, call.Return!, body);
                        resolve = true;
                    }
                    break;

                default:
                    throw new Exception();

            }
            return call.Return is { } ? LocalValueInferenceWithEffect(ns, m, call.Return!) || resolve : resolve;
        }

        public static INamespace GetStructNamespace(INamespace ns, IStructBody body)
        {
            switch (body)
            {
                case ExternStruct x:
                    return new NamespaceJunction(ns).Return(j => j.Uses.Add(x));

                case StructBody x:
                    return x.Namespace;

                case TypeSpecialization x:
                    return new NamespaceJunction(ns).Return(j => j.Uses.Add(x));
            }
            throw new Exception();
        }

        public static IEnumerable<string> GetStructNames(TypeMapper m, IEvaluable e)
        {
            if (e is TypeGenericsValue g)
            {
                if (g.Type is PropertyValue prop) return GetStructNames(m, prop.Left).Concat(prop.Right);
                return new string[] { g.Type.ToString()! };
            }
            if (m.ContainsKey(e) && m[e].Struct is NamespaceBody ns) return GetNamespaceNames(ns);
            throw new Exception();
        }

        public static IEnumerable<string> GetNamespaceNames(NamespaceBody ns) => ns.Parent is { } p ? GetNamespaceNames(p).Concat(ns.Name) : new string[] { ns.Name };

        public static void Feedback(IStructBody? left, IStructBody? right)
        {
            if (right is null) return;

            if (left is NumericStruct num)
            {
                if (num.Types.Contains(right)) num.Types.RemoveAll(x => x != right);
            }
        }

        public static bool ArgumentInferenceWithEffect(INamespace ns, TypeMapper m, IEvaluable v, ITypeDefinition type, int index)
        {
            if (m.ContainsKey(v) && m[v].Struct is { } p && IsDecideType(p)) return false;
            if (type is TypeGenericsParameter g)
            {
                if (!m.ContainsKey(type)) m[type] = CreateVariableDetail(g.Name, new GenericsParameter(g.Name), VariableType.TypeParameter, index);
                m[v] = CreateVariableDetail($"${index}", m[type].Struct, VariableType.Argument, index);
            }
            else if (type is TypeValue t)
            {
                m[v] = CreateVariableDetail($"${index}", Lookup.LoadStruct(ns, t.Name), VariableType.Argument, index);
            }
            else
            {
                throw new Exception();
            }
            return true;
        }

        public static bool TypeInferenceWithEffect(INamespace ns, TypeMapper m, IEvaluable v, ITypeDefinition type)
        {
            if (m.ContainsKey(v) && m[v].Struct is { } p && IsDecideType(p)) return false;
            if (type is TypeGenericsParameter gen)
            {
                m[v] = CreateVariableDetail("", m[gen].Struct, VariableType.TypeParameter);
            }
            else if (type is TypeGenericsValue g)
            {
                var gens = Lookup.TypeMapperToGenericsMapper(m);
                m[v] = CreateVariableDetail("", Lookup.GetArgumentType(ns, type, gens), VariableType.Type);
            }
            else if (type is TypeValue t)
            {
                m[v] = CreateVariableDetail("", Lookup.LoadStruct(ns, t.Name), VariableType.Type);
            }
            else if (type is TypeEnum te)
            {
                m[v] = CreateVariableDetail("", Lookup.LoadEnumStruct(ns, te), VariableType.Type);
            }
            else
            {
                throw new Exception();
            }
            return true;
        }

        public static bool LocalValueInferenceWithEffect(INamespace ns, TypeMapper m, IEvaluable v, IStructBody? b = null)
        {
            if (m.ContainsKey(v))
            {
                if ((m[v].Struct is { } p && IsDecideType(p)) || b is null) return false;
                m[v].Struct = b;
            }
            else
            {
                m[v] = CreateVariableDetail(v.ToString()!, b, VariableType.LocalVariable, m.Values.Where(x => x.Type == VariableType.LocalVariable).FoldLeft((r, x) => Math.Max(r, x.Index + 1), 0));
            }
            return true;
        }

        public static VariableDetail ToTypedValue(INamespace ns, TypeMapper m, IEvaluable v)
        {
            if (m.ContainsKey(v) && m[v].Struct is { } p && IsDecideType(p)) return m[v];

            switch (v)
            {
                case NumericValue x:
                    if (m.ContainsKey(v) && m[x].Struct is NumericStruct num) return m[x];
                    m[x] = CreateNumericType(ns);
                    return m[x];

                case StringValue x:
                    m[x] = CreateVariableDetail("", Lookup.LoadStruct(ns, "String"), VariableType.PrimitiveValue);
                    return m[x];

                case VariableValue x:
                    return m[x];

                case TemporaryValue x:
                    return m[x];

                case PropertyValue x:
                    _ = ToTypedValue(ns, m, x.Left);
                    var prop = m[x] = CreateVariableDetail(x.Right, GetPropertyType(m, x.Left, x.Right), VariableType.Property);
                    prop.Reciever = x.Left;
                    return m[x];

                case ArrayContainer x:
                    x.Values.Each(value => ToTypedValue(ns, m, value));
                    var gens = new List<IStructBody>() { m[x.Values[0]].Struct! };
                    m[x] = CreateVariableDetail("", Lookup.FindStructOrNull(ns, new string[] { "System", "Collections", "Generic", "List" }, gens), VariableType.PrimitiveValue);
                    return m[x];

                case TypeValue x:
                    m[x] = CreateVariableDetail(x.Name, new NamespaceBody(x.Name), VariableType.Namespace);
                    return m[x];
            }
            throw new Exception();
        }

        public static IStructBody? GetPropertyType(TypeMapper m, IEvaluable receiver, string property)
        {
            var left = m[receiver].Struct;
            switch (left)
            {
                case StructBody x:
                    return GetPropertyType(x, property, null);

                case TypeSpecialization x:
                    return GetPropertyType(x.Body, property, x.GenericsMapper);

                case NamespaceBody x:
                    return new NamespaceBody(property) { Parent = x };

                default:
                    throw new Exception();
            }
        }

        public static IStructBody? GetPropertyType(IStructBody left, string property, GenericsMapper? g)
        {
            switch (left)
            {
                case StructBody x:
                    if (x.Members.ContainsKey(property))
                    {
                        var m = g is null ? x.SpecializationMapper.First().Value : Lookup.GetGenericsTypeMapperOrNull(x.SpecializationMapper, g)!.Value.TypeMapper;
                        if (m.ContainsKey(x.Members[property])) return m[x.Members[property]].Struct;
                    }
                    break;

                default:
                    throw new Exception();
            }
            return null;
        }

        public static VariableDetail CreateNumericType(INamespace ns)
        {
            var num = new NumericStruct();
            num.Types.Add(Lookup.LoadStruct(ns, "Int"));
            num.Types.Add(Lookup.LoadStruct(ns, "Long"));
            num.Types.Add(Lookup.LoadStruct(ns, "Short"));
            num.Types.Add(Lookup.LoadStruct(ns, "Byte"));
            return CreateVariableDetail("", num, VariableType.PrimitiveValue);
        }

        public static VariableDetail CreateVariableDetail(string name, IStructBody? b, VariableType type, int index = 0) => new VariableDetail { Name = name, Struct = b, Type = type, Index = index };

        public static bool IsDecideType(IStructBody body)
        {
            switch (body)
            {
                case TypeSpecialization x:
                    return x.GenericsMapper.And(x => x.Value is { } p && IsDecideType(p));

                case NumericStruct x:
                    return x.Types.Count == 1;

                case FunctionMapper x when x.Function is ISpecialization sp:
                    var g = Lookup.TypeMapperToGenericsMapper(x.TypeMapper);
                    var mapper = Lookup.GetTypemapperOrNull(sp.SpecializationMapper, g);
                    return mapper is { };
            }
            return true;
        }
    }
}
