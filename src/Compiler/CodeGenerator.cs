using Extensions;
using Roku.IntermediateCode;
using Roku.Manager;
using Roku.TypeSystem;
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
            return e.Function.Cast<RkCILFunction>().MethodInfo.DeclaringType!.Assembly;
        }

        public static void AssemblyExternEmit(StreamWriter il, Assembly[] extern_asms)
        {
            extern_asms.Each(x =>
            {
                il.WriteLine($"//.assembly extern {x.GetName().FullName}");
            });
            il.WriteLine(".assembly extern mscorlib {}");
        }

        public static void AssemblyNameEmit(StreamWriter il, string path)
        {
            il.WriteLine($".assembly {Path.GetFileNameWithoutExtension(path)} {{}}");
        }

        public static void AssemblyFunctionEmit(StreamWriter il, IEnumerable<FunctionBody> fss)
        {
            fss.Each(x =>
            {
                il.WriteLine($".method public static void {x.Function.Name}() cil managed");
                il.WriteLine("{");
                il.WriteLine("\t.entrypoint");
                il.WriteLine("\t.maxstack 8");
                x.Body.Each(x => AssemblyOperandEmit(il, x));
                il.WriteLine("\tret");
                il.WriteLine("}");
            });
        }

        public static void AssemblyOperandEmit(StreamWriter il, Operand op)
        {
            switch (op)
            {
                case Call x:
                    il.WriteLine(x.Arguments.Map(LoadValue).Join('\n'));
                    il.WriteLine($"call void {GetFunctionName(x.Function!)}(string)");
                    break;
            }
        }

        public static string LoadValue(ITypedValue value)
        {
            switch (value)
            {
                case StringValue x:
                    return $"ldstr \"{x.Value}\"";
            }
            throw new Exception();
        }

        public static string GetFunctionName(IFunction f)
        {
            switch (f)
            {
                case RkCILFunction x:
                    return $"[mscorlib]{x.MethodInfo.DeclaringType!.FullName}::{x.MethodInfo.Name}";
            }
            throw new Exception();
        }

        public static string GetTypeName(ITypedValue v)
        {
            throw new Exception();
        }
    }
}
