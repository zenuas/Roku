using Mina.Extension;
using Roku.Manager;
using System.Linq;

namespace Roku.Compiler;

public static partial class Typing
{
    public static void TypeInference(RootNamespace root, SourceCodeBody src)
    {
        var srcs = Lookup.AllPrograms(src).ToList();

        while (true)
        {
            var resolved = false;

            Lookup.AllStructBodies(srcs).Concat(Lookup.AllStructBodies(root)).Each(x => resolved = TypeInference(x) || resolved);
            Lookup.AllFunctionBodies(srcs).Concat(Lookup.AllFunctionBodies(root)).Each(x => resolved = TypeInference(x) || resolved);

            if (!resolved) break;
        }
    }

    public static VariableDetail CreateVariableDetail(string name, IStructBody? b, VariableType type, int index = 0) => new() { Name = name, Struct = b, Type = type, Index = index };

    public static bool IsDecideType(IStructBody body)
    {
        switch (body)
        {
            case StructSpecialization x:
                return x.GenericsMapper.All(x => x.Value is { } p && IsDecideType(p));

            case NumericStruct x:
                return x.Types.Count == 1;

            case FunctionMapper x when x.Function is ISpecialization sp:
                return IsDecideFunction(x);
        }
        return true;
    }

    public static bool IsDecideFunction(IStructBody body)
    {
        switch (body)
        {
            case FunctionMapper x when x.Function is ISpecialization sp:
                var g = Lookup.TypeMapperToGenericsMapper(x.TypeMapper);
                var mapper = Lookup.GetTypemapperOrNull(sp.SpecializationMapper, g);
                return mapper is { };

            case FunctionMapper _:
                return true;
        }
        return false;
    }
}
