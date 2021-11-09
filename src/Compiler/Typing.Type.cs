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
        if (body.LexicalScope.ContainsKey("$self"))
        {
            self = body.LexicalScope["$self"].Cast<VariableValue>();
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
                if (mapper.ContainsKey(x))
                {
                    var d = mapper[x];
                    d.Type = VariableType.Property;
                    d.Reciever = self;
                    d.Index = 0;
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
