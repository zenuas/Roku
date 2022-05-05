﻿using Extensions;
using Roku.IntermediateCode;
using Roku.Manager;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Compiler;

public static partial class CodeGenerator
{
    public static void AssemblyStructEmit(ILWriter il, StructBody body, List<(string, FunctionSpecialization)> fss)
    {
        var cache = new HashSet<string>();
        body.SpecializationMapper.Each(sp =>
        {
            var g = sp.Key;
            var mapper = sp.Value;

            var name = GetStructName(body.Name, body, g);
            if (cache.Contains(name)) return;
            _ = cache.Add(name);

            il.WriteLine($".class public {name}");
            il.WriteLine("{");
            il.Indent++;
            body.Members.Each(x => il.WriteLine($".field public {GetTypeName(mapper, x.Value, g)} {EscapeILName(x.Key)}"));
            il.WriteLine("");

            il.WriteLine($".method public void .ctor()");
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
        });
    }
}
