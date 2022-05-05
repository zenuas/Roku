using Extensions;
using Roku.IntermediateCode;
using Roku.Manager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Roku.Compiler;

public static partial class CodeGenerator
{
    public static void AssemblyExternEmit(ILWriter il, IEnumerable<Assembly> extern_asms) => extern_asms.Each(x => il.WriteLine($".assembly extern {x.GetName().Name} {{}}"));

    public static void AssemblyNameEmit(ILWriter il, string path) => il.WriteLine($".assembly {Path.GetFileNameWithoutExtension(path)} {{}}");

    public static void StructsToFunctionList(IEnumerable<StructBody> structs, List<(string, FunctionSpecialization)> fss)
    {
        var cache = new HashSet<string>();
        structs.Each(body =>
        {
            body.SpecializationMapper.Each(sp =>
            {
                var g = sp.Key;
                var mapper = sp.Value;

                var name = GetStructName(body.Name, body, g);
                if (cache.Contains(name)) return;
                _ = cache.Add(name);

                body.Body.OfType<Call>().Each(x => CallToAddEmitFunctionList(mapper, x, fss));
            });
        });
    }

    public static void FunctionsToFunctionList(List<(string Name, FunctionSpecialization Function)> fss)
    {
        for (var i = 0; i < fss.Count; i++)
        {
            var f = fss[i].Function.Body.Cast<IFunctionBody>();
            var g = fss[i].Function.GenericsMapper;
            var mapper = Lookup.GetTypemapper(f.SpecializationMapper, g);

            var local_vals = mapper.Values.Where(x => x.Type == VariableType.LocalVariable && x.Struct is AnonymousFunctionBody anon && anon.Generics.Count == 0).Sort((a, b) => a.Index - b.Index).ToList();
            if (local_vals.Count > 0)
            {
                local_vals.Select(x => x.Struct).OfType<AnonymousFunctionBody>().Each(x => CallToAddEmitFunctionList(mapper, x, fss));
            }

            var local_vals2 = mapper.Values.Where(IsCallableAnonymousFunction).Sort((a, b) => a.Index - b.Index).ToList();
            if (local_vals2.Count > 0)
            {
                local_vals2.Each(x => CallToAddEmitFunctionList(mapper, x.Struct!.Cast<FunctionMapper>(), x.Struct!.Cast<FunctionMapper>().Function.Cast<AnonymousFunctionBody>(), fss));
            }
            f.Body.OfType<Call>().Each(x => CallToAddEmitFunctionList(mapper, x, fss));
        }
    }

    public static bool IsCallableAnonymousFunction(VariableDetail vd)
    {
        if (vd.Struct is FunctionMapper fm && fm.Function is AnonymousFunctionBody af)
        {
            if (af.Generics.Count == 0) return true;

            var g = Lookup.TypeMapperToGenericsMapper(fm.TypeMapper);
            if (g.Count > 0) return true;
        }
        return false;
    }

    public static void CallToAddEmitFunctionList(TypeMapper m, Call call, List<(string Name, FunctionSpecialization Function)> fss)
    {
        var f = m[call.Function.Function].Struct!.Cast<FunctionMapper>();

        if (f.Function is FunctionBody body)
        {
            var g = Lookup.TypeMapperToGenericsMapper(f.TypeMapper);
            if (fss.FindFirstIndex(x => EqualsFunctionCaller(x.Function, body, g)) < 0)
            {
                _ = AppendFunctionSpecialization(fss, new FunctionSpecialization(body, g));
            }
        }
    }

    public static void CallToAddEmitFunctionList(TypeMapper m, AnonymousFunctionBody anon, List<(string, FunctionSpecialization)> fss) => AppendFunctionSpecialization(fss, new FunctionSpecialization(anon, new GenericsMapper()));

    public static void CallToAddEmitFunctionList(TypeMapper m, FunctionMapper fm, AnonymousFunctionBody anon, List<(string, FunctionSpecialization)> fss) => AppendFunctionSpecialization(fss, new FunctionSpecialization(anon, Lookup.TypeMapperToGenericsMapper(fm.TypeMapper)));

    public static bool EqualsFunctionCaller(FunctionSpecialization left, IFunctionName right, GenericsMapper right_g)
    {
        if (left.Body != right) return false;
        return left.GenericsMapper.Keys.All(x => left.GenericsMapper[x] == right_g[x]);
    }
}
