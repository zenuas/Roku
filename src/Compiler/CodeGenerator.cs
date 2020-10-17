using Extensions;
using Roku.Declare;
using Roku.IntermediateCode;
using Roku.Manager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Roku.Compiler
{
    public static class CodeGenerator
    {
        public static void Emit(RootNamespace root, SourceCodeBody body, string path)
        {
            var pgms = Lookup.AllPrograms(body);
            var nss = Lookup.AllNamespaces(body);
            var entrypoint = Lookup.AllFunctionBodies(pgms).FindFirst(x => x.Name == "main");
            var structs = Lookup.AllStructBodies(pgms).Concat(Lookup.AllStructBodies(root));
            var externs = Lookup.AllExternFunctions(nss);
            var embedded = Lookup.AllEmbeddedFunctions(nss);
            var extern_structs = Lookup.AllExternStructs(Lookup.GetRootNamespace(body));
            var extern_asms = externs.Map(x => x.Assembly)
                .Concat(extern_structs.Map(x => x.Assembly))
                .By<Assembly>().Unique().ToArray();

            using var il = new ILWriter(path);
            AssemblyExternEmit(il, extern_asms);
            AssemblyNameEmit(il, path);

            var fss = new List<FunctionSpecialization>() { new FunctionSpecialization(entrypoint, new GenericsMapper()) };
            structs.Each(x => AssemblyStructEmit(il, x, fss));
            AssemblyFunctionEmit(il, fss);
        }

        public static Type GetType(ExternFunction e) => e.DeclaringType ?? e.Function.DeclaringType!;

        public static void AssemblyExternEmit(ILWriter il, Assembly[] extern_asms) => extern_asms.Each(x => il.WriteLine($".assembly extern {x.GetName().Name} {{}}"));

        public static void AssemblyNameEmit(ILWriter il, string path) => il.WriteLine($".assembly {Path.GetFileNameWithoutExtension(path)} {{}}");

        public static void AssemblyStructEmit(ILWriter il, StructBody body, List<FunctionSpecialization> fss)
        {
            var cache = new HashSet<string>();
            body.SpecializationMapper.Each(sp =>
            {
                var g = sp.Key;
                var mapper = sp.Value;

                var name = EscapeILName(body.Name, body, g);
                if (cache.Contains(name)) return;
                cache.Add(name);

                il.WriteLine($".class public {name}");
                il.WriteLine("{");
                il.Indent++;
                body.Members.Each(x => il.WriteLine($".field public {GetTypeName(mapper, x.Value, g)} {EscapeILName(x.Key)}"));
                il.WriteLine("");

                il.WriteLine($".method public void .ctor()");
                il.WriteLine("{");
                il.Indent++;
                var local_vals = mapper.Values.Where(x => x.Type == VariableType.LocalVariable && !(x.Struct is NamespaceBody)).Sort((a, b) => a.Index - b.Index).ToList();
                local_vals.Each((x, i) => x.Index = i);
                if (local_vals.Count > 0)
                {
                    il.WriteLine(".locals(");
                    il.Indent++;
                    il.WriteLine(local_vals.Map(x => $"[{x.Index}] {GetTypeName(x, g)} {x.Name}").Join(",\n"));
                    il.Indent--;
                    il.WriteLine(")");
                }
                var labels = Lookup.AllLabels(body.Body).Zip(Lists.Sequence(1)).ToDictionary(x => x.First, x => $"_{x.First.Name}{x.Second}");
                body.Body.Each(x =>
                {
                    if (x is Call call) CallToAddEmitFunctionList(mapper, call, fss);
                    AssemblyOperandEmit(il, x, body.Namespace, mapper, labels, g);
                });
                il.WriteLine("ret");
                il.Indent--;
                il.WriteLine("}");

                il.Indent--;
                il.WriteLine("}");
            });
        }

        public static void AssemblyFunctionEmit(ILWriter il, List<FunctionSpecialization> fss)
        {
            for (var i = 0; i < fss.Count; i++)
            {
                var f = fss[i].Body.Cast<FunctionBody>();
                var g = fss[i].GenericsMapper;
                var mapper = Lookup.GetTypemapper(f.SpecializationMapper, g);

                il.WriteLine($".method public static {GetTypeName(mapper, f.Return, g)} {EscapeILName(f.Name)}({f.Arguments.Map(a => GetTypeName(mapper[a.Name], g)).Join(", ")})");
                il.WriteLine("{");
                il.Indent++;
                if (i == 0) il.WriteLine(".entrypoint");
                il.WriteLine(".maxstack 8");
                var local_vals = mapper.Values.Where(x => x.Type == VariableType.LocalVariable && !(x.Struct is NamespaceBody)).Sort((a, b) => a.Index - b.Index).ToList();
                local_vals.Each((x, i) => x.Index = i);
                if (local_vals.Count > 0)
                {
                    il.WriteLine(".locals(");
                    il.Indent++;
                    il.WriteLine(local_vals.Map(x => $"[{x.Index}] {GetTypeName(x, g)} {x.Name}").Join(",\n"));
                    il.Indent--;
                    il.WriteLine(")");
                }
                var labels = Lookup.AllLabels(f.Body).Zip(Lists.Sequence(1)).ToDictionary(x => x.First, x => $"_{x.First.Name}{x.Second}");
                f.Body.Each(x =>
                {
                    if (x is Call call) CallToAddEmitFunctionList(mapper, call, fss);
                    AssemblyOperandEmit(il, x, f, mapper, labels, g);
                });
                il.WriteLine("ret");
                il.Indent--;
                il.WriteLine("}");
            }
        }

        public static void CallToAddEmitFunctionList(TypeMapper m, Call call, List<FunctionSpecialization> fss)
        {
            var f = m[call.Function.Function].Struct!.Cast<FunctionMapper>();

            if (f.Function is FunctionBody body)
            {
                var g = Lookup.TypeMapperToGenericsMapper(f.TypeMapper);
                if (fss.FindFirstIndex(x => EqualsFunctionCaller(x, body, g)) < 0)
                {
                    fss.Add(new FunctionSpecialization(body, g));
                }
            }
        }

        public static bool EqualsFunctionCaller(FunctionSpecialization left, IFunctionBody right, GenericsMapper right_g)
        {
            if (left.Body != right) return false;
            return left.GenericsMapper.Keys.And(x => left.GenericsMapper[x] == right_g[x]);
        }

        public static void AssemblyOperandEmit(ILWriter il, IOperand op, INamespace ns, TypeMapper m, Dictionary<LabelCode, string> labels, GenericsMapper g)
        {
            il.WriteLine();
            if (op is IReturnBind prop && prop.Return is { } && m[prop.Return].Type == VariableType.Property)
            {
                il.WriteLine(LoadValue(m, m[prop.Return].Reciever!));
            }

            switch (op.Operator)
            {
                case Operator.Bind:
                    if (op is IReturnBind r && m[r.Return!].Struct is NamespaceBody) return;
                    var bind = op.Cast<Code>();
                    il.WriteLine(LoadValue(m, bind.Left!));
                    break;

                case Operator.Call:
                    {
                        var call = op.Cast<Call>();
                        var f = m[call.Function.Function].Struct!.Cast<FunctionMapper>();
                        var args = call.Function.Arguments.Map((x, i) => LoadValue(m, x, GetArgumentType(ns, f, i))).ToArray();
                        if (f.Function is ExternFunction fx)
                        {
                            il.WriteLine(args.Join('\n'));
                            var callsig = fx.Function.IsVirtual ? "callvirt instance" : "call";
                            var retvar = GetParameterName(fx.Function.ReturnType);
                            var asmname = $"[{fx.Assembly.GetName().Name}]";
                            var classname = GetTypeName(GetType(fx), Lookup.TypeMapperToGenericsMapper(f.TypeMapper));
                            var fname = fx.Function.Name;
                            var param_args = fx.Function.GetParameters().Map(x => GetParameterName(x.ParameterType)).Join(", ");
                            il.WriteLine($"{callsig} {retvar} class {asmname}{classname}::{fname}({param_args})");
                        }
                        else if (f.Function is FunctionBody fb)
                        {
                            il.WriteLine(args.Join('\n'));
                            il.WriteLine($"call {GetTypeName(f.TypeMapper, fb.Return, g)} {EscapeILName(fb.Name)}({fb.Arguments.Map(a => GetTypeName(f.TypeMapper[a.Name], g)).Join(", ")})");
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

                case Operator.TypeBind:
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

        public static IStructBody? GetArgumentType(INamespace caller, FunctionMapper body, int index)
        {
            switch (body.Function)
            {
                case FunctionBody fb: return Lookup.GetStructType(caller, fb.Arguments[index].Type, body.TypeMapper);
                case EmbeddedFunction ef: return Lookup.GetStructType(caller, ef.Arguments[index], body.TypeMapper);
            }
            return null;
        }

        public static string LoadValue(TypeMapper m, IEvaluable value, IStructBody? target = null)
        {
            switch (value)
            {
                case StringValue x:
                    return $"ldstr \"{x.Value}\"";

                case NumericValue x:
                    {
                        var isbox = IsClassType(target);
                        if (m[x].Struct is ExternStruct es && es.Struct == typeof(long).GetTypeInfo())
                        {
                            return
                                (x.Value <= 8 ? $"ldc.i4.{x.Value}\nconv.i8"
                                : x.Value <= sbyte.MaxValue ? $"ldc.i4.s {x.Value}\nconv.i8"
                                : x.Value <= int.MaxValue ? $"ldc.i4 {x.Value}\nconv.i8"
                                : $"ldc.i8 {x.Value}") +
                                (isbox ? $"\nbox {GetStructName(m[x].Struct)}" : "");
                        }
                        else
                        {
                            return
                                (x.Value <= 8 ? $"ldc.i4.{x.Value}"
                                : x.Value <= sbyte.MaxValue ? $"ldc.i4.s {x.Value}"
                                : x.Value <= int.MaxValue ? $"ldc.i4 {x.Value}"
                                : $"ldc.i8 {x.Value}") +
                                (isbox ? $"\nbox {GetStructName(m[x].Struct)}" : "");
                        }
                    }

                case VariableValue _:
                case TemporaryValue _:
                    {
                        var detail = m[value];
                        var isbox = Lookup.IsValueType(detail.Struct) && IsClassType(target);
                        if (detail.Type == VariableType.Argument)
                        {
                            return
                                (detail.Index <= 3 ? $"ldarg.{detail.Index}"
                                : detail.Index <= byte.MaxValue ? $"ldarg.s {detail.Index}"
                                : $"ldarg {detail.Index}") +
                                (isbox ? $"\nbox {GetStructName(detail.Struct)}" : "");
                        }
                        else
                        {
                            return
                                (detail.Index <= 3 ? $"ldloc.{detail.Index}"
                                : detail.Index <= byte.MaxValue ? $"ldloc.s {detail.Index}"
                                : $"ldloc {detail.Index}") +
                                (isbox ? $"\nbox {GetStructName(detail.Struct)}" : "");
                        }
                    }

                case PropertyValue x:
                    return $"{LoadValue(m, x.Left)}\nldfld {GetStructName(m[x].Struct)} {GetStructName(m[x.Left].Struct)}::{EscapeILName(x.Right)}";

                case ArrayContainer x:
                    return $"newobj instance void {GetStructName(m[x].Struct)}::.ctor()\n{x.Values.Map(v => "dup\n" + LoadValue(m, v) + $"\ncallvirt instance void {GetStructName(m[x].Struct)}::Add(!0)").Join("\n")}";
            }
            throw new Exception();
        }

        public static string StoreValue(TypeMapper m, IEvaluable value)
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
                    else if (detail.Type == VariableType.LocalVariable)
                    {
                        return
                            detail.Index <= 3 ? $"stloc.{detail.Index}"
                            : detail.Index <= byte.MaxValue ? $"stloc.s {detail.Index}"
                            : $"stloc {detail.Index}";
                    }
                    else if (detail.Type == VariableType.Property)
                    {
                        return $"stfld {GetStructName(detail.Struct!)} {GetStructName(m[detail.Reciever!].Struct!)}::{EscapeILName(detail.Name)}";
                    }
                    break;

                case PropertyValue x:
                    return $"stfld {GetStructName(m[x].Struct)} {GetStructName(m[x.Left].Struct)}::{EscapeILName(x.Right)}";
            }
            throw new Exception();
        }

        public static string GetTypeName(TypeMapper m, IEvaluable? e, GenericsMapper g) => e is null ? "void" : GetStructName(GetType(m[e], g));

        public static string GetTypeName(VariableDetail vd, GenericsMapper g) => GetStructName(GetType(vd, g));

        public static string GetTypeName(Type t, GenericsMapper g) => t.FullName! + (!t.IsGenericType ? "" : $"<{t.GetGenericArguments().Map(x => GetStructName(g.GetValue(x.Name))).Join(", ")}>");

        public static string GetParameterName(Type t) =>
            t.IsGenericTypeParameter ? $"!{t.GenericParameterPosition}"
            : t.IsGenericMethodParameter ? $"!!{t.GenericParameterPosition}"
            : GetILStructName(t);

        public static IStructBody? GetType(VariableDetail vd, GenericsMapper g) => GetType(vd.Struct, g);

        public static IStructBody? GetType(IStructBody? body, GenericsMapper g) => body is GenericsParameter gp ? g.FindFirst(x => x.Key.Name == gp.Name).Value : body;

        public static string GetStructName(IStructBody? body)
        {
            switch (body)
            {
                case null: return "void";
                case ExternStruct x: return GetILStructName(x);
                case NumericStruct x: return GetStructName(x.Types.First());
                case StructBody x: return $"class {EscapeILName(x.Name)}";
                case StructSpecialization x when x.Body is ExternStruct e: return $"class [{e.Assembly.GetName().Name}]{e.Struct.FullName}{GetGenericsName(e, x.GenericsMapper)}";
                case StructSpecialization x when x.Body is StructBody e: return $"class {EscapeILName(x.Name, e, x.GenericsMapper)}";
                case EnumStructBody _: return "object";
            }
            throw new Exception();
        }

        public static string EscapeILName(string s) => Regex.IsMatch(s, "^_*[a-zA-Z][_a-zA-Z0-9]*$") ? s : $"'{s}'";

        public static string EscapeILName(string name, ISpecialization sp, GenericsMapper g) => g.Count == 0 ? name : $"'{name}{GetGenericsName(sp, g)}'";

        public static string GetGenericsName(ISpecialization sp, GenericsMapper g) => $"<{sp.Generics.Map(x => GetStructName(g[x])).Join(", ")}>";

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

        public static bool IsClassType(IStructBody? body) =>
            (body is { } && !Lookup.IsValueType(body));

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
