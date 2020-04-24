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
                    //var f = m[x.Function]!;
                    il.WriteLine(x.Arguments.Map(LoadValue).Join('\n'));
                    //il.WriteLine($"call {GetTypeName(m[f.Return])} {GetFunctionName(f)}({x.Arguments.Map(a => GetTypeName(a.Type)).Join(", ")})");
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

        //public static string GetFunctionName(IFunction f)
        //{
        //    switch (f)
        //    {
        //        case RkCILFunction x:
        //            return $"[{x.MethodInfo.DeclaringType!.FullName}]{x.MethodInfo.DeclaringType!.FullName}::{x.MethodInfo.Name}";

        //        case RkFunction x:
        //            return $"{x.Name}";
        //    }
        //    throw new Exception();
        //}

        public static string GetTypeName(IStructBody? t)
        {
            switch (t)
            {
                case null: return "void";
                case ExternStruct x when x.Struct == typeof(string): return "string";
            }
            throw new Exception();
        }
    }
}
