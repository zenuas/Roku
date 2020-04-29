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
            var fss = Lookup.AllFunctionBodies(pgms).UnZip().First;
            var externs = Lookup.AllExternFunctions(nss);
            var embedded = Lookup.AllEmbeddedFunctions(nss);
            var extern_asms = externs.Map(GetAssembly).Concat(embedded.Map(x => x.Assembly()).By<Assembly>()).Unique().ToArray();

            using (var il = new ILWriter(path))
            {
                AssemblyExternEmit(il, extern_asms);
                AssemblyNameEmit(il, path);
                AssemblyFunctionEmit(il, fss);
            }
        }

        public static Assembly GetAssembly(ExternFunction e)
        {
            return e.Function.DeclaringType!.Assembly;
        }

        public static void AssemblyExternEmit(ILWriter il, Assembly[] extern_asms)
        {
            extern_asms.Each(x => il.WriteLine($".assembly extern {x.GetName().Name} {{}}"));
        }

        public static void AssemblyNameEmit(ILWriter il, string path)
        {
            il.WriteLine($".assembly {Path.GetFileNameWithoutExtension(path)} {{}}");
        }

        public static void AssemblyFunctionEmit(ILWriter il, IEnumerable<FunctionBody> fss)
        {
            fss.Each(f =>
            {
                il.WriteLine($".method public static {GetTypeName(f.TypeMapper, f.Return)} {f.Name}({f.Arguments.Map(a => GetTypeName(f.TypeMapper[a.Name])).Join(", ")}) cil managed");
                il.WriteLine("{");
                il.Indent++;
                if (f.Name == "main") il.WriteLine(".entrypoint");
                il.WriteLine(".maxstack 8");
                var local_vals = f.TypeMapper.Values.Where(x => x.Type == VariableType.LocalVariable).Sort((a, b) => a.Index - b.Index).ToList();
                if (local_vals.Count > 0)
                {
                    il.WriteLine(".locals(");
                    il.Indent++;
                    il.WriteLine(local_vals.Map(x => $"[{x.Index}] {GetTypeName(x)} {x.Name}").Join(",\n"));
                    il.Indent--;
                    il.WriteLine(")");
                }
                var labels = Lookup.AllLabels(f).Zip(Lists.Sequence(1)).ToDictionary(x => x.First, x => $"_{x.First.Name}{x.Second}");
                f.Body.Each(x => AssemblyOperandEmit(il, x, f.TypeMapper, labels));
                il.WriteLine("ret");
                il.Indent--;
                il.WriteLine("}");
            });
        }

        public static void AssemblyOperandEmit(ILWriter il, IOperand op, Dictionary<ITypedValue, VariableDetail> m, Dictionary<LabelCode, string> labels)
        {
            il.WriteLine();
            switch (op.Operator)
            {
                case Operator.Bind:
                    var bind = op.Cast<Code>();
                    il.WriteLine(LoadValue(m, bind.Left!));
                    break;

                case Operator.Call:
                    var call = op.Cast<Call>();
                    var f = m[call.Function.Function].Struct!.Cast<FunctionMapper>();
                    var args = call.Function.Arguments.Map(x => LoadValue(m, x)).ToArray();
                    if (f.Function is ExternFunction fx)
                    {
                        il.WriteLine(args.Join('\n'));
                        il.WriteLine($"call {GetTypeName(fx.Function.ReturnType)} [{fx.Function.DeclaringType!.FullName}]{fx.Function.DeclaringType!.FullName}::{fx.Function.Name}({call.Function.Arguments.Map(a => GetTypeName(m[a])).Join(", ")})");
                    }
                    else if (f.Function is FunctionBody fb)
                    {
                        il.WriteLine(args.Join('\n'));
                        il.WriteLine($"call {GetTypeName(f.TypeMapper, fb.Return)} {fb.Name}({fb.Arguments.Map(a => GetTypeName(f.TypeMapper[a.Name])).Join(", ")})");
                    }
                    else if (f.Function is EmbeddedFunction ef)
                    {
                        il.WriteLine(ef.OpCode(args));
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

                default:
                    throw new Exception();
            }

            if (op is IReturnBind ret && ret.Return is { })
            {
                il.WriteLine(StoreValue(m, ret.Return));
            }
        }

        public static string LoadValue(Dictionary<ITypedValue, VariableDetail> m, ITypedValue value)
        {
            switch (value)
            {
                case StringValue x:
                    return $"ldstr \"{x.Value}\"";

                case NumericValue x:
                    return
                        x.Value <= 8 ? $"ldc.i4.{x.Value}"
                        : x.Value <= sbyte.MaxValue ? $"ldc.i4.s {x.Value}"
                        : x.Value <= int.MaxValue ? $"ldc.i4 {x.Value}"
                        : $"ldc.i8 {x.Value}";

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
            }
            throw new Exception();
        }

        public static string StoreValue(Dictionary<ITypedValue, VariableDetail> m, ITypedValue value)
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

        public static string GetTypeName(Dictionary<ITypedValue, VariableDetail> m, ITypedValue? t) => t is null ? "void" : GetTypeName(m[t]);

        public static string GetTypeName(VariableDetail t)
        {
            switch (t.Struct)
            {
                case null: return "void";
                case ExternStruct x: return GetTypeName(x.Struct);
            }
            throw new Exception();
        }

        public static string GetTypeName(Type t)
        {
            if (t == typeof(void)) return "void";
            if (t == typeof(string)) return "string";
            if (t == typeof(int)) return "int32";
            if (t == typeof(bool)) return "bool";
            return t.Name;
        }
    }
}
