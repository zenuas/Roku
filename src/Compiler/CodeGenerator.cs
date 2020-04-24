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
            var extern_asms = externs.Map(GetAssembly).ToArray();

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
                il.WriteLine($".method public static void {f.Name}({f.Arguments.Map(a => GetTypeName(f.TypeMapper[a.Name])).Join(", ")}) cil managed");
                il.WriteLine("{");
                if (f.Name == "main") il.WriteLine("\t.entrypoint");
                il.WriteLine("\t.maxstack 8");
                f.Body.Each(x => AssemblyOperandEmit(il, x, f.TypeMapper));
                il.WriteLine("\tret");
                il.WriteLine("}");
            });
        }

        public static void AssemblyOperandEmit(StreamWriter il, Operand op, Dictionary<ITypedValue, IStructBody?> m)
        {
            switch (op)
            {
                case Call x:
                    var f = m[x.Function]!.Cast<FunctionMapper>();
                    il.WriteLine(x.Arguments.Map(LoadValue).Join('\n'));
                    if (f.Function is ExternFunction fx)
                    {
                        il.WriteLine($"call {GetTypeName(fx.Function.ReturnType)} [{fx.Function.DeclaringType!.FullName}]{fx.Function.DeclaringType!.FullName}::{fx.Function.Name}({x.Arguments.Map(a => GetTypeName(m[a])).Join(", ")})");
                    }
                    else if (f.Function is FunctionBody fb)
                    {
                        il.WriteLine($"call {GetTypeName(f.TypeMapper, fb.Return)} {fb.Name}({fb.Arguments.Map(a => GetTypeName(f.TypeMapper[a.Name])).Join(", ")})");
                    }
                    break;
            }
        }

        public static string LoadValue(ITypedValue value)
        {
            switch (value)
            {
                case StringValue x:
                    return $"ldstr \"{x.Value}\"";

                case VariableValue x:
                    return $"ldarg.0";
            }
            throw new Exception();
        }

        public static string GetTypeName(Dictionary<ITypedValue, IStructBody?> m, ITypedValue? t) => t is null ? "void" : GetTypeName(m[t]);

        public static string GetTypeName(IStructBody? t)
        {
            switch (t)
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
            return t.Name;
        }
    }
}
