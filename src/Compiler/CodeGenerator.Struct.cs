using Extensions;
using Roku.IntermediateCode;
using Roku.Manager;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Compiler;

public static partial class CodeGenerator
{
    public static IEnumerable<(StructBody Body, GenericsMapper GenericsMapper)> AllAssemblyStructs(StructBody body)
    {
        var cache = new HashSet<string>();
        foreach (var sp in body.SpecializationMapper)
        {
            var g = sp.Key;
            var mapper = sp.Value;

            var name = GetStructName(body.Name, body, g);
            if (cache.Contains(name)) continue;
            _ = cache.Add(name);

            yield return (body, g);
        }
    }

    public static void AssemblyStructEmit(ILWriter il, StructBody body, GenericsMapper g, List<(string, FunctionSpecialization)> fss, Dictionary<StructBody, int> struct_index)
    {
        var mapper = body.SpecializationMapper[g];
        var name = GetStructName(body.Name, body, g);

        il.WriteLine($".class public {name}");
        il.WriteLine("{");
        il.Indent++;
        il.WriteLine(".field public static int32 '#type_no'");
        il.WriteLine(".field public static class [System.Runtime]System.Type[] '#type_generics'");
        il.WriteLine("");
        body.Members.Each(x => il.WriteLine($".field public {GetTypeName(mapper, x.Value, g)} {EscapeILName(x.Key)}"));
        il.WriteLine("");

        il.WriteLine(".method private static void .cctor()");
        il.WriteLine("{");
        il.Indent++;
        il.WriteLine($"ldc.i4.{struct_index[body]}");
        il.WriteLine($"stsfld int32 {name}::'#type_no'");
        il.WriteLine("");
        il.WriteLine($"ldc.i4.{body.Generics.Count}");
        il.WriteLine("newarr [System.Runtime]System.Type");
        body.Generics.Each((x, i) =>
        {
            il.WriteLine("");
            il.WriteLine("dup");
            il.WriteLine($"ldc.i4.{i}");
            il.WriteLine($"ldtoken {GetStructName(mapper[x].Struct)}");
            il.WriteLine("call class [System.Runtime]System.Type [System.Runtime]System.Type::GetTypeFromHandle(valuetype [System.Runtime]System.RuntimeTypeHandle)");
            il.WriteLine("stelem.ref");
        });
        il.WriteLine($"stsfld class [System.Runtime]System.Type[] {name}::'#type_generics'");
        il.WriteLine("ret");
        il.Indent--;
        il.WriteLine("}");
        il.WriteLine("");

        il.WriteLine(".method public void .ctor()");
        il.WriteLine("{");
        il.Indent++;
        var local_vals = GetLocalValues(mapper);
        if (local_vals.Count > 0)
        {
            il.WriteLine(".locals(");
            il.Indent++;
            il.WriteLine(local_vals.Select(x => $"[{x.Index}] {GetTypeName(x, g)} {x.Name}").Join(",\n"));
            il.Indent--;
            il.WriteLine(")");
        }
        var labels = Lookup.AllLabels(body.Body).Zip(Lists.Sequence(1)).ToDictionary(x => x.First, x => $"_{x.First.Name}{x.Second}");
        body.Body.Each(x => AssemblyOperandEmit(il, x, body.Namespace, mapper, labels, g, fss));
        il.WriteLine("ret");
        il.Indent--;
        il.WriteLine("}");

        il.Indent--;
        il.WriteLine("}");
    }
}
