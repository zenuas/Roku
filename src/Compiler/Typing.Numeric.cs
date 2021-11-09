using Extensions;
using Roku.Manager;
using System.Linq;

namespace Roku.Compiler;

public static partial class Typing
{
    public static void SpecializationNumericDecide(ISpecialization sp) => sp.SpecializationMapper.Values.Each(TypeMapperNumericDecide);

    public static void TypeMapperNumericDecide(TypeMapper m)
    {
        var nums = m.Where(x => x.Value.Struct is NumericStruct).ToList();
        for (var i = 0; i < nums.Count; i++)
        {
            nums[i].Value.Struct = nums[i].Value.Struct!.Cast<NumericStruct>().Types.First();
        }
        foreach (var fm in m.Where(x => x.Value.Struct is FunctionMapper).Select(x => x.Value.Struct!.Cast<FunctionMapper>()))
        {
            //if (fm.Function is ISpecialization sp) SpecializationNumericDecide(sp);
            TypeMapperNumericDecide(fm.TypeMapper);
        }
        foreach (var gm in m.Where(x => x.Value.Struct is IGenericsMapper).Select(x => x.Value.Struct!.Cast<IGenericsMapper>()))
        {
            var keys = gm.GenericsMapper.Keys.ToList();
            foreach (var key in keys)
            {
                var g = gm.GenericsMapper[key];
                if (g is NumericStruct num)
                {
                    gm.GenericsMapper[key] = num.Types.First();
                }
                else if (g is ISpecialization sp)
                {
                    SpecializationNumericDecide(sp);
                }
            }
        }
    }

    public static VariableDetail CreateNumericType(INamespace ns)
    {
        var num = new NumericStruct();
        num.Types.Add(Lookup.LoadStruct(ns, "Int"));
        num.Types.Add(Lookup.LoadStruct(ns, "Long"));
        num.Types.Add(Lookup.LoadStruct(ns, "Short"));
        num.Types.Add(Lookup.LoadStruct(ns, "Byte"));
        return CreateVariableDetail("", num, VariableType.PrimitiveValue);
    }

    public static VariableDetail CreateFloatingNumericType(INamespace ns)
    {
        var num = new NumericStruct();
        num.Types.Add(Lookup.LoadStruct(ns, "Double"));
        num.Types.Add(Lookup.LoadStruct(ns, "Float"));
        return CreateVariableDetail("", num, VariableType.PrimitiveValue);
    }
}
