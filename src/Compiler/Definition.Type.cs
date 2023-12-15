using Extensions;
using Roku.Declare;
using Roku.Manager;
using Roku.Node;

namespace Roku.Compiler;

public static partial class Definition
{
    public static void TypeDefinition(SourceCodeBody src, ProgramNode pgm) => pgm.Structs.Each(x => src.Structs.Add(TypeBodyDefinition(src, x)));

    public static StructBody TypeBodyDefinition(SourceCodeBody src, StructNode sn)
    {
        var body = new StructBody(src, sn.Name.Name);
        sn.Generics.Each(x => body.Generics.Add(new TypeGenericsParameter() { Name = x.Name }));
        FunctionBodyDefinition(body, sn.Statements);
        sn.Statements.Each(let =>
        {
            switch (let)
            {
                case LetNode x:
                    body.Members.Add(x.Var.Name, body.LexicalScope[x.Var.Name]);
                    break;

                case LetTypeNode x:
                    body.Members.Add(x.Var.Name, body.LexicalScope[x.Var.Name]);
                    break;

                default:
                    throw new();
            }
        });
        if (sn.Generics.Count == 0)
        {
            body.SpecializationMapper[[]] = [];
            src.Functions.Add(new EmbeddedFunction(sn.Name.Name, sn.Name.Name) { OpCode = (_, args) => $"newobj instance void {sn.Name.Name}::.ctor()" });
        }
        return body;
    }
}
