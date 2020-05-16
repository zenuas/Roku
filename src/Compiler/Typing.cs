﻿using Extensions;
using Roku.IntermediateCode;
using Roku.Manager;
using System;
using System.Reflection;

namespace Roku.Compiler
{
    public static class Typing
    {
        public static void TypeInference(SourceCodeBody src)
        {
            var srcs = Lookup.AllPrograms(src).ToList();
            Lookup.AllStructBodies(srcs).Each(TypeInference);
            Lookup.AllFunctionBodies(srcs).Each(TypeInference);
        }

        public static void TypeInference(StructBody body)
        {
            while (StructBodyInference(body)) ;
            SpecializationNumericDecide(body);
            while (StructBodyInference(body)) ;

            var self = new VariableValue("$self");
            foreach (var mapper in body.SpecializationMapper.Values)
            {
                body.LexicalScope.Add(self.Name, self);
                mapper[self] = CreateVariableDetail(self.Name, body, VariableType.Argument, 0);

                body.Members.Values.Each(x =>
                {
                    var d = mapper[x];
                    d.Type = VariableType.Property;
                    d.Reciever = self;
                    d.Index = 0;
                });
            }
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

        public static void TypeInference(FunctionBody body)
        {
            while (FunctionBodyInference(body)) ;
            SpecializationNumericDecide(body);
            while (FunctionBodyInference(body)) ;

            foreach (var mapper in body.SpecializationMapper.Values)
            {
                if (body.Body.By<IfCastCode>().FindFirstOrNull(x => Lookup.IsValueType(mapper[x.Condition].Struct)) is { })
                {
                    LocalValueInferenceWithEffect(body.Namespace, mapper, mapper.CastBoxCondition, Lookup.LoadType(Lookup.GetRootNamespace(body.Namespace), typeof(object)));
                }
            }
        }

        public static bool FunctionBodyInference(FunctionBody body)
        {
            var resolved = false;
            var keys = body.SpecializationMapper.Keys.ToArray();
            for (var i = 0; i < keys.Length; i++)
            {
                var value = body.SpecializationMapper[keys[i]];
                body.Arguments.Each((x, i) => resolved = ArgumentInferenceWithEffect(body.Namespace, value, x.Name, x.Type, i) || resolved);
                if (body.Return is { } x) resolved = TypeInferenceWithEffect(body.Namespace, value, x, x.Name) || resolved;
                body.Body.Each(x => resolved = OperandTypeInference(body.Namespace, value, x) || resolved);
            }
            return resolved;
        }

        public static void SpecializationNumericDecide(ISpecialization sp)
        {
            foreach (var value in sp.SpecializationMapper.Values)
            {
                var nums = value.Where(x => x.Value.Struct is NumericStruct).ToList();
                for (var i = 0; i < nums.Count; i++)
                {
                    nums[i].Value.Struct = nums[i].Value.Struct!.Cast<NumericStruct>().Types.First();
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
                    return LocalValueInferenceWithEffect(ns, m, x.Name, Lookup.LoadStruct(ns, x.Type.Name));

                case IfCastCode x:
                    return LocalValueInferenceWithEffect(ns, m, x.Name, Lookup.LoadStruct(ns, x.Type.Name));

                case IfCode _:
                case GotoCode _:
                case LabelCode _:
                    return false;
            }
            throw new Exception();
        }

        public static bool ResolveFunctionWithEffect(INamespace ns, TypeMapper m, Call call)
        {
            if (m.ContainsKey(call.Function.Function) && m[call.Function.Function] is { }) return false;

            var resolve = false;
            var args = call.Function.Arguments.Map(x => ToTypedValue(ns, m, x).Struct).ToList();
            switch (call.Function.Function)
            {
                case VariableValue x when call.Return is null && call.Function.FirstLookup is null && x.Name == "return":
                    {
                        var ret = new EmbeddedFunction("return", null, args.Map(x => x?.Name!).ToArray()) { OpCode = (args) => $"{(args.Length == 0 ? "" : args[0] + "\n")}ret" };
                        var fm = new FunctionMapper(ret);
                        m[x] = CreateVariableDetail("", fm, VariableType.FunctionMapper);
                        call.Caller = new FunctionCaller(ret, new GenericsMapper());
                        return true;
                    }

                case VariableValue x:
                    var body = Lookup.FindFunctionOrNull(ns, x.Name, args);
                    if (body is { })
                    {
                        var fm = new FunctionMapper(body.Body);
                        IStructBody? ret = null;
                        if (body.Body is FunctionBody fb)
                        {
                            if (Lookup.GetTypemapperOrNull(fb.SpecializationMapper, body.GenericsMapper) is null) fb.SpecializationMapper[body.GenericsMapper] = GenericsMapperToTypeMapper(body.GenericsMapper);
                            if (fb.Return is { }) fm.TypeMapper[fb.Return] = CreateVariableDetail("", ret = Lookup.GetArgumentType(fb.Namespace, fb.Return, body.GenericsMapper), VariableType.Type);
                            fb.Arguments.Each((x, i) => fm.TypeMapper[x.Name] = CreateVariableDetail(x.Name.Name, Lookup.GetArgumentType(fb.Namespace, x.Type, body.GenericsMapper), VariableType.Argument, i));
                            fb.Arguments.Each((x, i) => Feedback(args[i], fm.TypeMapper[x.Name].Struct));
                        }
                        else if (body.Body is ExternFunction fx)
                        {
                            ret = Lookup.LoadTypeWithoutVoid(Lookup.GetRootNamespace(ns), fx.Function.ReturnType.GetTypeInfo());
                        }
                        else if (body.Body is EmbeddedFunction ef)
                        {
                            if (ef.Return is { }) fm.TypeMapper[ef.Return] = CreateVariableDetail("", ret = Lookup.LoadStruct(ns, ef.Return.Name), VariableType.Type);
                            ef.Arguments.Each((x, i) => fm.TypeMapper[x] = CreateVariableDetail($"${i}", Lookup.LoadStruct(ns, x.Name), VariableType.Argument, i));
                        }
                        m[x] = CreateVariableDetail("", fm, VariableType.FunctionMapper);
                        call.Caller = body;
                        if (call.Return is { }) LocalValueInferenceWithEffect(ns, m, call.Return!, ret);
                        resolve = true;
                    }
                    break;

            }
            return call.Return is { } ? LocalValueInferenceWithEffect(ns, m, call.Return!) || resolve : resolve;
        }

        public static void Feedback(IStructBody? left, IStructBody? right)
        {
            if (right is null) return;

            if (left is NumericStruct num)
            {
                if (num.Types.Contains(right)) num.Types.RemoveAll(x => x != right);
            }
        }

        public static TypeMapper GenericsMapperToTypeMapper(GenericsMapper g)
        {
            var mapper = new TypeMapper();
            g.Each(kv => mapper[kv.Key] = CreateVariableDetail(kv.Key.Name, kv.Value, VariableType.TypeParameter));
            return mapper;
        }

        public static bool ArgumentInferenceWithEffect(INamespace ns, TypeMapper m, ITypedValue v, TypeValue type, int index)
        {
            if (m.ContainsKey(v) && m[v].Struct is { }) return false;
            if (type.Types == Types.Generics)
            {
                if (!m.ContainsKey(type)) m[type] = CreateVariableDetail(type.Name, new GenericsParameter(type.Name), VariableType.TypeParameter, index);
                m[v] = CreateVariableDetail($"${index}", m[type].Struct, VariableType.Argument, index);
            }
            else
            {
                m[v] = CreateVariableDetail($"${index}", Lookup.LoadStruct(ns, type.Name), VariableType.Argument, index);
            }
            return true;
        }

        public static bool TypeInferenceWithEffect(INamespace ns, TypeMapper m, ITypedValue v, string type_name)
        {
            if (m.ContainsKey(v) && m[v].Struct is { }) return false;
            m[v] = CreateVariableDetail("", Lookup.LoadStruct(ns, type_name), VariableType.Type);
            return true;
        }

        public static bool LocalValueInferenceWithEffect(INamespace ns, TypeMapper m, ITypedValue v, IStructBody? b = null)
        {
            if (m.ContainsKey(v))
            {
                if (m[v].Struct is { } || b is null) return false;
                m[v].Struct = b;
            }
            else
            {
                m[v] = CreateVariableDetail(v.ToString()!, b, VariableType.LocalVariable, m.Values.Where(x => x.Type == VariableType.LocalVariable).FoldLeft((r, x) => Math.Max(r, x.Index + 1), 0));
            }
            return true;
        }

        public static VariableDetail ToTypedValue(INamespace ns, TypeMapper m, ITypedValue v)
        {
            if (m.ContainsKey(v)) return m[v];

            switch (v)
            {
                case NumericValue x:
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
                    var prop = m[x] = CreateVariableDetail(x.Right, GetPropertyType(ns, m, x.Left, x.Right), VariableType.Property);
                    prop.Reciever = x.Left;
                    return m[x];

                case ArrayContainer x:
                    x.Values.Each(value => ToTypedValue(ns, m, value));
                    m[x] = CreateVariableDetail("", Lookup.LoadStruct(ns, "ListInt"), VariableType.PrimitiveValue);
                    return m[x];
            }
            throw new Exception();
        }

        public static IStructBody? GetPropertyType(INamespace ns, TypeMapper m, ITypedValue receiver, string property)
        {
            var left = m[receiver].Struct;
            switch (left)
            {
                case StructBody x:
                    if (x.Members.ContainsKey(property)) return x.SpecializationMapper.First().Value[x.Members[property]].Struct;
                    break;
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
    }
}
