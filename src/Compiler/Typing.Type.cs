using Extensions;
using Roku.Declare;
using Roku.Manager;
using System.Linq;

namespace Roku.Compiler;

public static partial class Typing
{
    public static bool TypeInference(StructBody body)
    {
        var resolved = StructBodyInference(body);
        SpecializationNumericDecide(body);
        _ = StructBodyInference(body);

        VariableValue self;
        if (body.LexicalScope.TryGetValue("$self", out var value))
        {
            self = value.Cast<VariableValue>();
        }
        else
        {
            self = new VariableValue() { Name = "$self" };
            body.LexicalScope.Add(self.Name, self);
        }

        foreach (var mapper in body.SpecializationMapper.Values)
        {
            if (!mapper.ContainsKey(self)) mapper[self] = CreateVariableDetail(self.Name, new StructSpecialization(body, Lookup.TypeMapperToGenericsMapper(mapper)), VariableType.Argument, 0);

            body.Members.Values.Each(x =>
            {
                if (mapper.TryGetValue(x, out var value))
                {
                    value.Type = VariableType.Property;
                    value.Reciever = self;
                    value.Index = 0;
                }
            });
        }
        return resolved;
    }

    public static bool StructBodyInference(StructBody body)
    {
        var resolved = false;
        var keys = body.SpecializationMapper.Keys.ToArray();
        for (var i = 0; i < keys.Length; i++)
        {
            var m = body.SpecializationMapper[keys[i]];
            body.Body.Each(x => resolved = OperandTypeInference(body.Namespace, m, x) || resolved);
        }
        return resolved;
    }
}
