using Extensions;
using Roku.IntermediateCode;
using Roku.Manager;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Roku.Compiler;

public static partial class CodeGenerator
{
    public static void Emit(RootNamespace root, SourceCodeBody body, string path)
    {
        var pgms = Lookup.AllPrograms(body);
        var nss = Lookup.AllNamespaces(body);
        var entrypoint = Lookup.AllFunctionBodies(pgms).First(x => x.Name == "main");
        var structs = Lookup.AllStructBodies(pgms).Concat(Lookup.AllStructBodies(root)).Where(Garbage.IncompleteType);
        var externs = Lookup.AllExternFunctions(nss);
        var embedded = Lookup.AllEmbeddedFunctions(nss);
        var instances = Lookup.AllInstanceBodies(pgms);
        var extern_structs = Lookup.AllExternStructs(Lookup.GetRootNamespace(body));
        var extern_asms = externs.Select(x => x.Assembly)
            .Concat(extern_structs.Select(x => x.Assembly))
            .OfType<Assembly>()
            .Distinct();

        var fss = new List<(string Name, FunctionSpecialization Function)>();
        _ = AppendFunctionSpecialization(fss, new() { Body = entrypoint, GenericsMapper = [] });
        StructsToFunctionList(structs, fss);
        FunctionsToFunctionList(fss);
        if (fss.FindFirstOrNullValue(x => x.Function.Body is AnonymousFunctionBody) is { })
        {
            extern_asms = extern_asms.Concat(Assembly.Load("mscorlib"));
        }

        using var il = new ILWriter(path);
        AssemblyExternEmit(il, extern_asms);
        AssemblyNameEmit(il, path);

        RuntimeEmit(il);
        InstanceEmit(instances);

        var struct_index = structs.Select((x, i) => (x, i)).ToDictionary(v => v.x, v => v.i);
        structs
            .Select(AllAssemblyStructs)
            .Flatten()
            .Each(x => AssemblyStructEmit(il, x.Body, x.GenericsMapper, fss, struct_index));
        AssemblyFunctionEmit(il, fss);
    }

    public static void RuntimeEmit(ILWriter il)
    {
        il.WriteLine(@"
.class interface public abstract '#RTTI'
{
    .method public hidebysig newslot abstract virtual instance int32 '#GetTypeNo'() {}
    .method public hidebysig newslot abstract virtual instance class [System.Runtime]System.Type[] '#GetTypeGenerics'() {}
}
");
    }

    public static void InstanceEmit(IEnumerable<InstanceBody> instances)
    {
        if (instances.IsEmpty()) return;
        throw new("not support");
    }
}
