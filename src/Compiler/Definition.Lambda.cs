using Mina.Extensions;
using Roku.Declare;
using Roku.Manager;
using Roku.Node;

namespace Roku.Compiler;

public static partial class Definition
{
    public static AnonymousFunctionBody LambdaExpressionDefinition(ILexicalScope scope, LambdaExpressionNode lambda)
    {
        var fbody = MakeAnonymousFunction(scope.Namespace, scope);
        fbody.IsImplicit = lambda.IsImplicit;
        if (lambda.Return is { } ret) fbody.Return = CreateType(fbody, ret);
        if (fbody.IsImplicit) fbody.Return = new TypeImplicit();
        lambda.Arguments.Each((x, i) =>
            {
                ITypeDefinition p;
                if (x is DeclareNode decla)
                {
                    p = CreateType(fbody, decla.Type);
                }
                else
                {
                    p = new TypeGenericsParameter() { Name = $"$type{i}" };
                    fbody.LexicalScope[p.Name] = p;
                }
                if (p is TypeGenericsParameter g) fbody.Generics.Add(g);
                var name = new VariableValue { Name = x.Name.Name };
                fbody.Arguments.Add((name, p));
                fbody.LexicalScope.Add(x.Name.Name, name);
            });

        if (fbody.Generics.Count == 0) fbody.SpecializationMapper[[]] = [];
        FunctionBodyDefinition(fbody, lambda.Statements);
        return fbody;
    }

    public static AnonymousFunctionBody MakeAnonymousFunction(IManaged ns, ILexicalScope scope)
    {
        var root = Lookup.GetRootNamespace(ns);
        var body = new AnonymousFunctionBody { Namespace = ns, Name = $"anonymous#{root.AnonymousFunctionUniqueCount++}", Parent = scope };
        root.Structs.Add(body);
        root.Functions.Add(body);
        return body;
    }
}
