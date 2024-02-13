using Mina.Extension;
using Roku.Manager;
using Roku.Node;

namespace Roku.Compiler;

public static partial class Definition
{
    public static void ClassDefinition(SourceCodeBody src, ProgramNode pgm) => pgm.Classes.Each(x => src.Classes.Add(ClassBodyDefinition(src, x)));

    public static ClassBody ClassBodyDefinition(SourceCodeBody src, ClassNode cn)
    {
        var body = new ClassBody { Namespace = src, Name = cn.Name.Name };
        cn.Generics.Each(x => body.Generics.Add(new() { Name = x.Name }));
        cn.Functions.Each(x => MakeFunctionDefinition(body, body.Generics, x));
        return body;
    }
}
