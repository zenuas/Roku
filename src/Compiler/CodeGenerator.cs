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

            using (var il = new StreamWriter(path))
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

        public static void AssemblyExternEmit(StreamWriter il, Assembly[] extern_asms)
        {
            extern_asms.Each(x => il.WriteLine($".assembly extern {x.GetName().Name} {{}}"));
        }

        public static void AssemblyNameEmit(StreamWriter il, string path)
        {
            il.WriteLine($".assembly {Path.GetFileNameWithoutExtension(path)} {{}}");
        }

        public static void AssemblyFunctionEmit(StreamWriter il, IEnumerable<FunctionBody> fss)
        {
            fss.Each(f =>
            {
                il.WriteLine($".method public static {GetTypeName(f.TypeMapper, f.Return)} {f.Name}({f.Arguments.Map(a => GetTypeName(f.TypeMapper[a.Name])).Join(", ")}) cil managed");
                il.WriteLine("{");
                if (f.Name == "main") il.WriteLine("\t.entrypoint");
                il.WriteLine("\t.maxstack 8");
                var local_vals = f.TypeMapper.Values.Where(x => x.Type == VariableType.LocalVariable).Sort((a, b) => a.Index - b.Index).ToList();
                if (local_vals.Count > 0)
                {
                    il.WriteLine(".locals(");
                    il.WriteLine(local_vals.Map(x => $"[{x.Index}] {GetTypeName(x)}").Join(",\n"));
                    il.WriteLine(")");
                }
                f.Body.Each(x => AssemblyOperandEmit(il, x, f.TypeMapper));
                il.WriteLine("\tret");
                il.WriteLine("}");
            });
        }

        public static void AssemblyOperandEmit(StreamWriter il, IOperand op, Dictionary<ITypedValue, VariableDetail> m)
        {
            il.WriteLine();
            switch (op.Operator)
            {
                case Operator.Bind:
                    var bind = op.Cast<Code>();
                    il.WriteLine(LoadValue(m, bind.Right!));
                    break;

                case Operator.Call:
                    var call = op.Cast<Call>();
                    var f = m[call.Function.Function].Struct!.Cast<FunctionMapper>();
                    il.WriteLine(call.Function.Arguments.Map(x => LoadValue(m, x)).Join('\n'));
                    if (f.Function is ExternFunction fx)
                    {
                        il.WriteLine($"call {GetTypeName(fx.Function.ReturnType)} [{fx.Function.DeclaringType!.FullName}]{fx.Function.DeclaringType!.FullName}::{fx.Function.Name}({call.Function.Arguments.Map(a => GetTypeName(m[a])).Join(", ")})");
                    }
                    else if (f.Function is FunctionBody fb)
                    {
                        il.WriteLine($"call {GetTypeName(f.TypeMapper, fb.Return)} {fb.Name}({fb.Arguments.Map(a => GetTypeName(f.TypeMapper[a.Name])).Join(", ")})");
                    }
                    else if (f.Function is EmbeddedFunction ef)
                    {
                        il.WriteLine(ef.OpCode());
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

        public static string LoadValue(Dictionary<ITypedValue, VariableDetail> m, ITypedValue value)
        {
            switch (value)
            {
                case StringValue x:
                    return $"ldstr \"{x.Value}\"";

                case NumericValue x:
                    return $"ldc.i4 {x.Value}";

                case VariableValue _:
                case TemporaryValue _:
                    var detail = m[value];
                    if (detail.Type == VariableType.Argument)
                    {
                        return $"ldarg.{detail.Index}";
                    }
                    else
                    {
                        return $"ldloc.{detail.Index}";
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
                        return $"starg.{detail.Index}";
                    }
                    else
                    {
                        return $"stloc.{detail.Index}";
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
            return t.Name;
        }
    }
}
