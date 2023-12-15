using Extensions;
using Roku.Declare;
using Roku.IntermediateCode;
using Roku.Manager;
using System.Linq;

namespace Roku.Compiler;

public static partial class Definition
{
    public static void CaptureDefinition(SourceCodeBody src)
    {
        src.Functions
            .OfType<FunctionBody>()
            .Where(IsCapturedFunction)
            .Each(f => MakeLexicalCapture(src, f));

        src.Structs
            .OfType<StructBody>()
            .Where(x => x.Type == StructBodyTypes.Capture)
            .Each(x => src.Functions.Add(new EmbeddedFunction(x.Name, x.Name) { OpCode = (_, args) => $"newobj instance void {CodeGenerator.EscapeILName(x.Name)}::.ctor()" }));
    }

    public static bool IsCapturedFunction(FunctionBody f) => f.Capture.Count > 0;

    public static bool IsScopeCapturedArgumentName(string name) => name.StartsWith("$##");

    public static string ScopeToUniqueName(ILexicalScope scope) => scope is FunctionBody fb ? $"##{fb.Name}" : throw new();

    public static void MakeLexicalCapture(SourceCodeBody src, FunctionBody f)
    {
        StructBody up(ILexicalScope current, VariableValue v, ILexicalScope find)
        {
            if (current == find)
            {
                var captured = current.LexicalScope[v.Name];
                current.LexicalScope.Remove(v.Name);
                var name = ScopeToUniqueName(current);
                var typename = new TypeValue() { Name = v.Name };
                var varname = $"${name}";
                if (src.Structs.OfType<StructBody>().FirstOrDefault(s => s.Name == name) is { } p)
                {
                    var scopevar = current.LexicalScope[varname];
                    CapturedVariableToProperty(current, captured, scopevar, v.Name);
                    p.LexicalScope.Add(v.Name, typename);
                    p.Members.Add(v.Name, typename);
                    return p;
                }
                else
                {
                    var scope = new StructBody(src, name) { Type = StructBodyTypes.Capture };
                    src.Structs.Add(scope);
                    scope.LexicalScope.Add(v.Name, typename);
                    scope.Members.Add(v.Name, typename);

                    var scopevar = new VariableValue() { Name = varname };
                    current.LexicalScope.Add(scopevar.Name, scopevar);
                    CapturedVariableToProperty(current, captured, scopevar, v.Name);
                    current.Body.Insert(0, new Call(new() { Function = new VariableValue() { Name = name } }) { Return = scopevar });

                    scope.SpecializationMapper[[]] = [];
                    return scope;
                }
            }
            else
            {
                var scope = up(current.Parent!, v, find);
                var argname = $"${scope.Name}";
                if (current is FunctionBody fb && !fb.Arguments.Exists(x => x.Name.Name == argname))
                {
                    var scopevar = new VariableValue() { Name = argname };
                    fb.Arguments.Insert(0, (scopevar, new TypeValue() { Name = scope.Name }));
                    fb.LexicalScope.Add(scope.Name, scopevar);
                }
                return scope;
            }
        }

        f.Capture.Each(kv =>
        {
            _ = up(f, kv.Key, kv.Value);

            var scopevar = f.LexicalScope[ScopeToUniqueName(kv.Value)];
            CapturedVariableToProperty(f, kv.Key, scopevar, kv.Key.Name);
        });
    }

    public static void CapturedVariableToProperty(ILexicalScope scope, IEvaluable original, IEvaluable scopevar, string name)
    {
        scope.Body.ForEach(x => CapturedVariableToPropertyOperand(scope, x, original, scopevar, name));
    }

    public static void CapturedVariableToPropertyOperand(ILexicalScope scope, IOperand ope, IEvaluable original, IEvaluable scopevar, string name)
    {
        switch (ope)
        {
            case BindCode bind:
                bind.Return = IfCapturedVariableToProperty(original, bind.Return, scopevar, name);
#pragma warning disable CS0612
                bind.LeftReplace(IfCapturedVariableToProperty(original, bind.Value, scopevar, name));
#pragma warning restore CS0612
                return;

            case IfCode if_:
#pragma warning disable CS0612
                if_.ConditionReplace(IfCapturedVariableToProperty(original, if_.Condition, scopevar, name)!);
#pragma warning restore CS0612
                return;

            case IfCastCode ifcast:
#pragma warning disable CS0612
                ifcast.ConditionReplace(IfCapturedVariableToProperty(original, ifcast.Condition, scopevar, name)!);
#pragma warning restore CS0612
                return;

            case Call call:
                for (var i = 0; i < call.Function.Arguments.Count; i++)
                {
                    call.Function.Arguments[i] = IfCapturedVariableToProperty(original, call.Function.Arguments[i], scopevar, name)!;
                }
                call.Return = IfCapturedVariableToProperty(original, call.Return, scopevar, name);
                return;
        }
        throw new();
    }

    public static IEvaluable? IfCapturedVariableToProperty(IEvaluable original, IEvaluable? v, IEvaluable scopevar, string name)
    {
        if (v is { } p && p == original) return new PropertyValue { Left = scopevar, Right = name };
        return v;
    }
}
