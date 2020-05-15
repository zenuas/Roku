using Extensions;
using Roku.IntermediateCode;
using Roku.Manager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Roku.Compiler
{
    public static class CodeGenerator
    {
        public static void Emit(SourceCodeBody body, string path)
        {
            var pgms = Lookup.AllPrograms(body);
            var nss = Lookup.AllNamespaces(body);
            var entrypoint = Lookup.AllFunctionBodies(pgms).FindFirst(x => x.Name == "main");
            var structs = Lookup.AllStructBodies(pgms);
            var externs = Lookup.AllExternFunctions(nss);
            var embedded = Lookup.AllEmbeddedFunctions(nss);
            var extern_structs = Lookup.AllExternStructs(Lookup.GetRootNamespace(body));
            var extern_asms = externs.Map(x => x.Assembly)
                .Concat(extern_structs.Map(x => x.Assembly))
                .By<Assembly>().Unique().ToArray();

            using (var il = new ILWriter(path))
            {
                AssemblyExternEmit(il, extern_asms);
                AssemblyNameEmit(il, path);
                structs.Each(x => AssemblyStructEmit(il, x));
                AssemblyFunctionEmit(il, entrypoint);
            }
        }

        public static Type GetType(ExternFunction e) => e.DeclaringType ?? e.Function.DeclaringType!;

        public static void AssemblyExternEmit(ILWriter il, Assembly[] extern_asms) => extern_asms.Each(x => il.WriteLine($".assembly extern {x.GetName().Name} {{}}"));

        public static void AssemblyNameEmit(ILWriter il, string path) => il.WriteLine($".assembly {Path.GetFileNameWithoutExtension(path)} {{}}");

        public static void AssemblyStructEmit(ILWriter il, StructBody body)
        {
            body.SpecializationMapper.Each(sp =>
            {
                var g = sp.Key;
                var mapper = sp.Value;

                il.WriteLine($".class public {body.Name}");
                il.WriteLine("{");
                il.Indent++;
                body.Members.Each(x => il.WriteLine($".field public {GetTypeName(mapper, x.Value, g)} {x.Key}"));
                il.WriteLine("");

                il.WriteLine($".method public void .ctor()");
                il.WriteLine("{");
                il.Indent++;
                il.WriteLine(".maxstack 8");
                il.WriteLine("ldarg.0");
                il.WriteLine("ldc.i4 123456");
                il.WriteLine($"stfld int32 {body.Name}::x");
                il.WriteLine("ret");
                il.Indent--;
                il.WriteLine("}");

                il.Indent--;
                il.WriteLine("}");
            });
            //il.WriteLine("newobj instance void Foo::.ctor()");
            //il.WriteLine("ldfld int32 Foo::x");
            //il.WriteLine("call void [System.Console]System.Console::WriteLine(int32)");
        }

        public static void AssemblyFunctionEmit(ILWriter il, FunctionBody entrypoint)
        {
            var fss = new List<FunctionCaller>() { new FunctionCaller(entrypoint, new GenericsMapper()) };
            for (var i = 0; i < fss.Count; i++)
            {
                var f = fss[i].Body.Cast<FunctionBody>();
                var g = fss[i].GenericsMapper;
                var mapper = Lookup.GetTypemapper(f.SpecializationMapper, g);

                il.WriteLine($".method public static {GetTypeName(mapper, f.Return, g)} {f.Name}({f.Arguments.Map(a => GetTypeName(mapper[a.Name], g)).Join(", ")})");
                il.WriteLine("{");
                il.Indent++;
                if (f == entrypoint) il.WriteLine(".entrypoint");
                il.WriteLine(".maxstack 8");
                var local_vals = mapper.Values.Where(x => x.Type == VariableType.LocalVariable).Sort((a, b) => a.Index - b.Index).ToList();
                if (local_vals.Count > 0)
                {
                    il.WriteLine(".locals(");
                    il.Indent++;
                    il.WriteLine(local_vals.Map(x => $"[{x.Index}] {GetTypeName(x, g)} {x.Name}").Join(",\n"));
                    il.Indent--;
                    il.WriteLine(")");
                }
                var labels = Lookup.AllLabels(f).Zip(Lists.Sequence(1)).ToDictionary(x => x.First, x => $"_{x.First.Name}{x.Second}");
                f.Body.Each(x =>
                {
                    if (x is Call call) CallToAddEmitFunctionList(call, fss);
                    AssemblyOperandEmit(f, il, x, mapper, labels, g);
                });
                il.WriteLine("ret");
                il.Indent--;
                il.WriteLine("}");
            }
        }

        public static void CallToAddEmitFunctionList(Call call, List<FunctionCaller> fss)
        {
            if (call.Caller is { } caller &&
                caller.Body is FunctionBody &&
                fss.FindFirstIndex(x => EqualsFunctionCaller(x, caller)) < 0)
            {
                fss.Add(caller);
            }
        }

        public static bool EqualsFunctionCaller(FunctionCaller left, FunctionCaller right)
        {
            if (left.Body != right.Body) return false;
            return left.GenericsMapper.Keys.And(x => left.GenericsMapper[x] == right.GenericsMapper[x]);
        }

        public static void AssemblyOperandEmit(FunctionBody body, ILWriter il, IOperand op, TypeMapper m, Dictionary<LabelCode, string> labels, GenericsMapper g)
        {
            il.WriteLine();
            switch (op.Operator)
            {
                case Operator.Bind:
                    var bind = op.Cast<Code>();
                    il.WriteLine(LoadValue(m, bind.Left!));
                    break;

                case Operator.Call:
                    {
                        var call = op.Cast<Call>();
                        var f = m[call.Function.Function].Struct!.Cast<FunctionMapper>();
                        var args = call.Function.Arguments.Map(x => LoadValue(m, x)).ToArray();
                        if (f.Function is ExternFunction fx)
                        {
                            il.WriteLine(args.Join('\n'));
                            il.WriteLine($"call {GetILStructName(fx.Function.ReturnType)} [{fx.Assembly.GetName().Name}]{GetType(fx).FullName}::{fx.Function.Name}({call.Function.Arguments.Map(a => GetTypeName(m[a], g)).Join(", ")})");
                        }
                        else if (f.Function is FunctionBody fb)
                        {
                            il.WriteLine(args.Join('\n'));
                            il.WriteLine($"call {GetTypeName(f.TypeMapper, fb.Return, g)} {fb.Name}({fb.Arguments.Map(a => GetTypeName(f.TypeMapper[a.Name], g)).Join(", ")})");
                        }
                        else if (f.Function is EmbeddedFunction ef)
                        {
                            il.WriteLine(ef.OpCode(args));
                        }
                    }
                    break;

                case Operator.If:
                    var if_ = op.Cast<IfCode>();
                    il.WriteLine(LoadValue(m, if_.Condition));
                    il.WriteLine($"brfalse {labels[if_.Else]}");
                    break;

                case Operator.Goto:
                    var goto_ = op.Cast<GotoCode>();
                    il.WriteLine($"br {labels[goto_.Label]}");
                    break;

                case Operator.Label:
                    var label = op.Cast<LabelCode>();
                    il.Indent--;
                    il.WriteLine($"{labels[label]}:");
                    il.Indent++;
                    break;

                case Operator.IfCast:
                    {
                        var ifcast = op.Cast<IfCastCode>();

                        var is_value = Lookup.IsValueType(m[ifcast.Condition].Struct);
                        var load_cond = LoadValue(m, ifcast.Condition);
                        il.WriteLine(load_cond);
                        if (is_value)
                        {
                            var value_type = m[ifcast.Condition].Struct!.Cast<ExternStruct>();

                            il.WriteLine($"box {GetILClassName(value_type)}");
                            il.WriteLine(StoreValue(m, m.CastBoxCondition));
                            il.WriteLine(load_cond = LoadValue(m, m.CastBoxCondition));
                        }

                        if (m[ifcast.Name].Struct is ExternStruct sx)
                        {
                            il.WriteLine($"isinst {GetILClassName(sx)}");

                            if (Lookup.IsValueType(m[ifcast.Name].Struct))
                            {
                                il.WriteLine($"brfalse.s {labels[ifcast.Else]}");
                                il.WriteLine(load_cond);
                                il.WriteLine($"unbox.any {GetILClassName(sx)}");
                                il.WriteLine(StoreValue(m, ifcast.Name));
                            }
                            else
                            {
                                il.WriteLine(StoreValue(m, ifcast.Name));
                                il.WriteLine(LoadValue(m, ifcast.Name));
                                il.WriteLine($"ldnull");
                                il.WriteLine($"cgt.un");
                                il.WriteLine($"brfalse.s {labels[ifcast.Else]}");
                            }
                        }
                    }
                    break;

                default:
                    throw new Exception();
            }

            if (op is IReturnBind ret && ret.Return is { })
            {
                il.WriteLine(StoreValue(m, ret.Return));
            }
        }

        public static string LoadValue(TypeMapper m, ITypedValue value)
        {
            switch (value)
            {
                case StringValue x:
                    return $"ldstr \"{x.Value}\"";

                case NumericValue x:
                    if (m[x].Struct is ExternStruct es && es.Struct == typeof(long).GetTypeInfo())
                    {
                        return
                            x.Value <= 8 ? $"ldc.i4.{x.Value}\nconv.i8"
                            : x.Value <= sbyte.MaxValue ? $"ldc.i4.s {x.Value}\nconv.i8"
                            : x.Value <= int.MaxValue ? $"ldc.i4 {x.Value}\nconv.i8"
                            : $"ldc.i8 {x.Value}";
                    }
                    else
                    {
                        return
                            x.Value <= 8 ? $"ldc.i4.{x.Value}"
                            : x.Value <= sbyte.MaxValue ? $"ldc.i4.s {x.Value}"
                            : x.Value <= int.MaxValue ? $"ldc.i4 {x.Value}"
                            : $"ldc.i8 {x.Value}";
                    }

                case VariableValue _:
                case TemporaryValue _:
                    var detail = m[value];
                    if (detail.Type == VariableType.Argument)
                    {
                        return
                            detail.Index <= 3 ? $"ldarg.{detail.Index}"
                            : detail.Index <= byte.MaxValue ? $"ldarg.s {detail.Index}"
                            : $"ldarg {detail.Index}";
                    }
                    else
                    {
                        return
                            detail.Index <= 3 ? $"ldloc.{detail.Index}"
                            : detail.Index <= byte.MaxValue ? $"ldloc.s {detail.Index}"
                            : $"ldloc {detail.Index}";
                    }

                case ArrayContainer x:
                    return $"newobj instance void {GetStructName(m[x].Struct)}::.ctor()\n{x.Values.Map(v => "dup\n" + LoadValue(m, v) + $"\ncallvirt instance void {GetStructName(m[x].Struct)}::Add(!0)").Join("\n")}";
            }
            throw new Exception();
        }

        public static string StoreValue(TypeMapper m, ITypedValue value)
        {
            switch (value)
            {
                case VariableValue _:
                case TemporaryValue _:
                    var detail = m[value];
                    if (detail.Type == VariableType.Argument)
                    {
                        return
                            detail.Index <= 3 ? $"starg.{detail.Index}"
                            : detail.Index <= byte.MaxValue ? $"starg.s {detail.Index}"
                            : $"starg {detail.Index}";
                    }
                    else
                    {
                        return
                            detail.Index <= 3 ? $"stloc.{detail.Index}"
                            : detail.Index <= byte.MaxValue ? $"stloc.s {detail.Index}"
                            : $"stloc {detail.Index}";
                    }
            }
            throw new Exception();
        }

        public static string GetTypeName(TypeMapper m, ITypedValue? t, GenericsMapper g) => t is null ? "void" : GetStructName(GetType(m[t], g));

        public static string GetTypeName(VariableDetail vd, GenericsMapper g) => GetStructName(GetType(vd, g));

        public static IStructBody? GetType(VariableDetail vd, GenericsMapper g) => GetType(vd.Struct, g);

        public static IStructBody? GetType(IStructBody? body, GenericsMapper g) => body is GenericsParameter gp ? g.FindFirst(x => x.Key.Name == gp.Name).Value : body;

        public static string GetStructName(IStructBody? body)
        {
            switch (body)
            {
                case null: return "void";
                case ExternStruct x: return GetILStructName(x);
            }
            throw new Exception();
        }

        public static string GetILStructName(ExternStruct sx) => GetILStructName(sx.Struct, sx.Assembly);

        public static string GetILStructName(Type t, Assembly? asm = null)
        {
            if (t == typeof(void)) return "void";
            if (t == typeof(string)) return "string";
            if (t == typeof(int)) return "int32";
            if (t == typeof(long)) return "int64";
            if (t == typeof(short)) return "int16";
            if (t == typeof(byte)) return "byte";
            if (t == typeof(bool)) return "bool";
            if (t == typeof(object)) return "object";
            return GetILClassName(t, asm ?? t.Assembly);
        }

        public static string GetILClassName(ExternStruct sx) => GetILClassName(sx.Struct, sx.Assembly);

        public static string GetILClassName(Type t, Assembly asm)
        {
            var gen = "";
            var gens = t.GetGenericArguments();
            if (gens.Length > 0)
            {
                gen = $"<{gens.Map(x => GetILStructName(x)).Join(", ")}>";
            }
            return $"{(t.IsValueType ? "" : "class ")}[{asm.GetName().Name}]{t.Namespace}.{t.Name}{gen}";
        }
    }
}
