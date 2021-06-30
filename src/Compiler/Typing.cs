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

                Lookup.AllStructBodies(srcs).Concat(Lookup.AllStructBodies(root)).Each(x => resolved = TypeInference(x) || resolved);
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
                if (!mapper.ContainsKey(self)) mapper[self] = CreateVariableDetail(self.Name, new StructSpecialization(body, Lookup.TypeMapperToGenericsMapper(mapper)), VariableType.Argument, 0);

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
                var m = body.SpecializationMapper[keys[i]];
                body.Body.Each(x => resolved = OperandTypeInference(body.Namespace, m, x) || resolved);
            }
            return resolved;
        }

        public static bool TypeInference(IFunctionBody body)
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

        public static bool FunctionBodyInference(IFunctionBody body)
        {
            var resolved = false;
            var keys = body.SpecializationMapper.Keys.ToArray();
            for (var i = 0; i < keys.Length; i++)
            {
                var g = keys[i];
                var m = body.SpecializationMapper[g];
                body.Arguments.Each((x, i) => resolved = ArgumentInferenceWithEffect(body.Namespace, m, x.Name, x.Type, i) || resolved);
                if (body.Return is { } x && !(x is TypeImplicit)) resolved = TypeInferenceWithEffect(body.Namespace, m, x, x) || resolved;
                body.Body.Each(x => resolved = OperandTypeInference(body, m, x) || resolved);

                if (body is AnonymousFunctionBody afb && afb.IsImplicit && body.Return is TypeImplicit v)
                {
                    if (!(m.ContainsKey(v) && m[v].Struct is { } p && IsDecideType(p)) &&
                        m.Keys.FindFirstOrNull(x => x is ImplicitReturnValue) is { } imp)
                    {
                        m[v] = CreateVariableDetail("", m[imp].Struct, VariableType.Type);
                        resolved = true;
                    }
                }

                if (body.Return is TypeGenericsParameter r)
                {
                    if (!(m.ContainsKey(r) && m[r].Struct is { } p && IsDecideType(p)))
                    {
                        var rets = m.Values
                            .Where(x => x.Struct is FunctionMapper fm && fm.Function is EmbeddedFunction && fm.Name == "return")
                            .Map(x => x.Struct!.Cast<FunctionMapper>().TypeMapper)
                            .Where(x => x.ContainsKey(r))
                            .Map(x => x[r].Struct)
                            .Unique()
                            .ToList();

                        if (rets.Count == 1)
                        {
                            g[r] = rets[0];
                            m[r] = CreateVariableDetail("", rets[0], VariableType.TypeParameter);
                            resolved = true;
                        }
                        else if (rets.Count > 1)
                        {

                        }
                    }
                }
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
            foreach (var gm in m.Where(x => x.Value.Struct is IGenericsMapper).Map(x => x.Value.Struct!.Cast<IGenericsMapper>()))
            {
                var keys = gm.GenericsMapper.Keys.ToList();
                foreach (var key in keys)
                {
                    var g = gm.GenericsMapper[key];
                    if (g is NumericStruct num)
                    {
                        gm.GenericsMapper[key] = num.Types.First();
                    }
                    else if (g is ISpecialization sp)
                    {
                        SpecializationNumericDecide(sp);
                    }
                }
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

        public static FunctionSpecialization? FindCurrentFunction(INamespace ns, TypeMapper m, VariableValue x, List<IStructBody?> args)
        {
            if (m.ContainsKey(x) && m[x].Struct is { } p)
            {
                switch (p)
                {
                    case FunctionTypeBody ftb: return new FunctionSpecialization(ftb, Lookup.FunctionArgumentsEquals(ns, ftb, args).GenericsMapper);
                }
            }
            return Lookup.FindFunctionOrNull(ns, x.Name, args);
        }

        public static bool ResolveFunctionWithEffect(INamespace ns, TypeMapper m, Call call)
        {
            if (m.ContainsKey(call.Function.Function) && m[call.Function.Function].Struct is { } p && IsDecideFunction(p) &&
                (call.Return is null || (call.Return is { } rx && m.ContainsKey(rx) && m[rx].Struct is { } rs && IsDecideType(rs))))
            {
                return false;
            }

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
                        if (r is TypeGenericsParameter gen) fm.TypeMapper[gen] = CreateVariableDetail("", m[gen].Struct ?? (args.Count > 0 ? args[0] : null), VariableType.TypeParameter);
                        else if (r is TypeSpecialization gv && m.ContainsKey(r) && m[r].Struct is StructSpecialization sp && sp.Body is ISpecialization sp2) gv.Generics.Each((x, i) => fm.TypeMapper[x] = CreateVariableDetail("", sp.GenericsMapper[sp2.Generics[i]], VariableType.TypeParameter));

                        m[x] = CreateVariableDetail("", fm, VariableType.FunctionMapper);
                        return true;
                    }

                case VariableValue x:
                    {
                        var caller = FindCurrentFunction(lookupns, m, x, args);
                        if (caller is null) break;

                        var fm = CreateFunctionMapper(ns, caller);
                        IStructBody? ret = null;
                        if (caller.Body is FunctionBody fb)
                        {
                            if (fb.Return is { }) ret = fm.TypeMapper[fb.Return].Struct;
                            fb.Arguments.Each((x, i) => Feedback(args[i], fm.TypeMapper[x.Name].Struct));
                        }
                        else if (caller.Body is FunctionTypeBody ftb)
                        {
                            ret = ftb.Return;
                        }
                        else if (caller.Body is ExternFunction fx)
                        {
                            ret = Lookup.LoadTypeWithoutVoid(Lookup.GetRootNamespace(ns), fx.Function.ReturnType, caller.GenericsMapper);
                        }
                        else if (caller.Body is EmbeddedFunction ef)
                        {
                            if (ef.Return is { }) ret = fm.TypeMapper[ef.Return].Struct;
                        }
                        m[x] = CreateVariableDetail("", fm, m.ContainsKey(x) ? m[x].Type : VariableType.FunctionMapper);
                        if (call.Return is { }) LocalValueInferenceWithEffect(ns, m, call.Return!, ret);
                        resolve = true;
                    }
                    break;

                case TypeSpecialization x:
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

        public static FunctionMapper CreateFunctionMapper(INamespace ns, FunctionSpecialization caller)
        {
            var fm = new FunctionMapper(caller.Body);
            caller.GenericsMapper.Each(p => fm.TypeMapper[p.Key] = Typing.CreateVariableDetail(p.Key.Name, p.Value, VariableType.TypeParameter));

            if (caller.Body is FunctionBody fb)
            {
                if (fb.Return is { } && !fm.TypeMapper.ContainsKey(fb.Return)) fm.TypeMapper[fb.Return] = Typing.CreateVariableDetail("", Lookup.GetStructType(fb.Namespace, fb.Return, caller.GenericsMapper), VariableType.Type);
                fb.Arguments.Each((x, i) => fm.TypeMapper[x.Name] = Typing.CreateVariableDetail(x.Name.Name, Lookup.GetStructType(fb.Namespace, x.Type, caller.GenericsMapper), VariableType.Argument, i));
            }
            else if (caller.Body is EmbeddedFunction ef)
            {
                if (ef.Return is { } && !fm.TypeMapper.ContainsKey(ef.Return)) fm.TypeMapper[ef.Return] = Typing.CreateVariableDetail("", Lookup.LoadStruct(ns, ef.Return.Name), VariableType.Type);
                ef.Arguments.Each((x, i) => fm.TypeMapper[x] = Typing.CreateVariableDetail($"${i}", Lookup.LoadStruct(ns, x.Name), VariableType.Argument, i));
            }
            return fm;
        }

        public static INamespace GetStructNamespace(INamespace ns, IStructBody body)
        {
            switch (body)
            {
                case ExternStruct x:
                    return new NamespaceJunction(ns).Return(j => j.Uses.Add(x));

                case StructBody x:
                    return x.Namespace;

                case StructSpecialization x:
                    return new NamespaceJunction(ns).Return(j => j.Uses.Add(x));
            }
            throw new Exception();
        }

        public static IEnumerable<string> GetStructNames(TypeMapper m, IEvaluable e)
        {
            if (e is TypeSpecialization g)
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
            else if (left is AnonymousFunctionBody anon)
            {
                Lookup.AppendSpecialization(anon, new GenericsMapper());
                if (anon.IsImplicit && anon.Return is TypeImplicit &&
                    right is FunctionTypeBody ftb && ftb.Return is null)
                {
                    anon.Return = null;
                    anon.IsImplicit = false;
                }
            }
        }

        public static bool ArgumentInferenceWithEffect(INamespace ns, TypeMapper m, IEvaluable v, ITypeDefinition type, int index)
        {
            if (m.ContainsKey(v) && m[v].Struct is { } p && IsDecideType(p)) return false;
            switch (type)
            {
                case TypeGenericsParameter g:
                    if (!m.ContainsKey(type)) m[type] = CreateVariableDetail(g.Name, new GenericsParameter(g.Name), VariableType.TypeParameter, index);
                    m[v] = CreateVariableDetail($"${index}", m[type].Struct, VariableType.Argument, index);
                    break;

                case TypeValue t:
                    m[v] = CreateVariableDetail($"${index}", Lookup.LoadStruct(ns, t.Name), VariableType.Argument, index);
                    break;

                case TypeSpecialization sp:
                    m[v] = CreateVariableDetail($"${index}", Lookup.FindStructOrNull(ns, new string[] { sp.Name }, sp.Generics.Map(x => Lookup.GetStructType(ns, x, m)!).ToList()), VariableType.Argument, index);
                    break;

                case TypeFunction tf:
                    m[v] = CreateVariableDetail($"${index}", Lookup.LoadFunctionType(ns, tf, Lookup.TypeMapperToGenericsMapper(m)), VariableType.Argument, index);
                    break;

                default:
                    throw new Exception();
            }
            return true;
        }

        public static bool TypeInferenceWithEffect(INamespace ns, TypeMapper m, IEvaluable v, ITypeDefinition type)
        {
            if (m.ContainsKey(v) && m[v].Struct is { } p && IsDecideType(p)) return false;
            var s = Lookup.GetStructType(ns, type, m);
            m[v] = CreateVariableDetail("", s, type is TypeGenericsParameter ? VariableType.TypeParameter : VariableType.Type);
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
                    if (m.ContainsKey(v) && m[x].Struct is NumericStruct) return m[x];
                    m[x] = CreateNumericType(ns);
                    return m[x];

                case FloatingNumericValue x:
                    if (m.ContainsKey(v) && m[x].Struct is NumericStruct) return m[x];
                    m[x] = CreateFloatingNumericType(ns);
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

                case FunctionReferenceValue x:
                    m[x] = CreateVariableDetail("", Lookup.GetRootNamespace(ns).Structs.By<AnonymousFunctionBody>().FindFirst(f => f.Name == x.Name), VariableType.PrimitiveValue);
                    return m[x];
            }
            throw new Exception();
        }

        public static IStructBody? GetPropertyType(TypeMapper m, IEvaluable receiver, string property)
        {
            var left = m[receiver].Struct;
            if (left is null) return null;

            switch (left)
            {
                case StructBody x:
                    return GetPropertyType(x, property, null);

                case StructSpecialization x:
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

        public static VariableDetail CreateFloatingNumericType(INamespace ns)
        {
            var num = new NumericStruct();
            num.Types.Add(Lookup.LoadStruct(ns, "Double"));
            num.Types.Add(Lookup.LoadStruct(ns, "Float"));
            return CreateVariableDetail("", num, VariableType.PrimitiveValue);
        }

        public static VariableDetail CreateVariableDetail(string name, IStructBody? b, VariableType type, int index = 0) => new VariableDetail { Name = name, Struct = b, Type = type, Index = index };

        public static bool IsDecideType(IStructBody body)
        {
            switch (body)
            {
                case StructSpecialization x:
                    return x.GenericsMapper.And(x => x.Value is { } p && IsDecideType(p));

                case NumericStruct x:
                    return x.Types.Count == 1;

                case FunctionMapper x when x.Function is ISpecialization sp:
                    return IsDecideFunction(x);
            }
            return true;
        }

        public static bool IsDecideFunction(IStructBody body)
        {
            switch (body)
            {
                case FunctionMapper x when x.Function is ISpecialization sp:
                    var g = Lookup.TypeMapperToGenericsMapper(x.TypeMapper);
                    var mapper = Lookup.GetTypemapperOrNull(sp.SpecializationMapper, g);
                    return mapper is { };

                case FunctionMapper _:
                    return true;
            }
            return false;
        }
    }
}
