using Extensions;
using Roku.Declare;
using Roku.Manager;
using Roku.Node;

namespace Roku.Compiler;

public static partial class Definition
{
    public static void InstanceDefinition(SourceCodeBody src, ProgramNode pgm) => pgm.Instances.Each(x => src.Instances.Add(InstanceBodyDefinition(src, x)));

    public static InstanceBody InstanceBodyDefinition(SourceCodeBody src, InstanceNode inst)
    {
        var body = new InstanceBody(src);
        body.Specialization = CreateTypeSpecialization(body, inst.Specialization);

        inst.InstanceMap.Each(x => InstanceMapDefinition(body, x));
        return body;
    }

    public static FunctionBody InstanceMapDefinition(InstanceBody inst, InstanceMapNode map)
    {
        var body = MakeFunction(inst, map.Name.Name);
        map.Arguments.Each(x => body.Arguments.Add((new VariableValue() { Name = x.Name.Name }, x is DeclareNode decla ? CreateType(inst, decla.Type) : new TypeImplicit())));
        body.Arguments.Each(x => body.LexicalScope.Add(x.Name.Name, x.Name));
        body.SpecializationMapper[new GenericsMapper()] = new TypeMapper();
        FunctionBodyDefinition(body, map.Statements);
        return body;
    }
}
