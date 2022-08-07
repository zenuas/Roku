using Extensions;
using Roku.Manager;
using Roku.Declare;
using System.Linq;
using System;

namespace Roku.Compiler;

public static partial class Definition
{
    public static void CaptureDefinition(SourceCodeBody src)
    {
        src.Functions
            .OfType<FunctionBody>()
            .Where(IsCapturedFunction)
            .Each(f => MakeLexicalCapture(src, f));
    }

    public static bool IsCapturedFunction(FunctionBody f) => f.Capture.Count > 0;

    public static string ScopeToUniqueName(ILexicalScope scope) => scope is FunctionBody fb ? $"##{fb.Name}" : throw new Exception();

    public static void MakeLexicalCapture(SourceCodeBody src, FunctionBody f)
    {
        StructBody up(ILexicalScope current, VariableValue v, ILexicalScope find)
        {
            if (current == find)
            {
                var name = ScopeToUniqueName(current);
                var typename = new TypeValue() { Name = v.Name };
                if (src.Structs.OfType<StructBody>().FirstOrDefault(s => s.Name == name) is { } p)
                {
                    p.LexicalScope.Add(v.Name, typename);
                    return p;
                }
                else
                {
                    var scope = new StructBody(src, name);
                    src.Structs.Add(scope);
                    scope.LexicalScope.Add(v.Name, typename);
                    return scope;
                }
            }
            else
            {
                var scope = up(current.Parent!, v, find);
                var argname = $"${scope.Name}";
                if (current is FunctionBody fb && !fb.Arguments.Exists(x => x.Name.Name == argname))
                {
                    fb.Arguments.Insert(0, (new VariableValue() { Name = argname }, new TypeValue() { Name = scope.Name }));
                }
                return scope;
            }
        }

        f.Capture.Each(kv => up(f, kv.Key, kv.Value));
    }
}
