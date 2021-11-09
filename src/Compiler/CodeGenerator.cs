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
        var entrypoint = Lookup.AllFunctionBodies(pgms).FindFirst(x => x.Name == "main");
        var structs = Lookup.AllStructBodies(pgms).Concat(Lookup.AllStructBodies(root)).Where(Garbage.IncompleteType);
        var externs = Lookup.AllExternFunctions(nss);
        var embedded = Lookup.AllEmbeddedFunctions(nss);
        var extern_structs = Lookup.AllExternStructs(Lookup.GetRootNamespace(body));
        var extern_asms = externs.Select(x => x.Assembly)
            .Concat(extern_structs.Select(x => x.Assembly))
            .OfType<Assembly>()
            .Distinct();

        var fss = new List<FunctionSpecialization>() { new FunctionSpecialization(entrypoint, new GenericsMapper()) };
        fss = fss.Concat(StructsToFunctionList(structs)).ToList();
        fss = FunctionsToFunctionList(fss);
        if (fss.FindFirstOrNull(x => x.Body is AnonymousFunctionBody) is { })
        {
            extern_asms = extern_asms.Concat(Assembly.Load("mscorlib"));
        }

        using var il = new ILWriter(path);
        AssemblyExternEmit(il, extern_asms);
        AssemblyNameEmit(il, path);

        structs.Each(x => AssemblyStructEmit(il, x));
        AssemblyFunctionEmit(il, fss);
    }
}
