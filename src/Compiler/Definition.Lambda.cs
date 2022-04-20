﻿using Extensions;
using Roku.Declare;
using Roku.Manager;
using Roku.Node;

namespace Roku.Compiler;

public static partial class Definition
{
    public static AnonymousFunctionBody LambdaExpressionDefinition(ILexicalScope scope, LambdaExpressionNode lambda)
    {
        var fbody = MakeAnonymousFunction(scope.Namespace);
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
                    p = new TypeGenericsParameter() { Name = x.Name.Name };
                    fbody.LexicalScope[$"$type{i}"] = p;
                }
                if (p is TypeGenericsParameter g) fbody.Generics.Add(g);
                var name = new VariableValue() { Name = x.Name.Name };
                fbody.Arguments.Add((name, p));
                fbody.LexicalScope.Add(x.Name.Name, name);
            });

        if (fbody.Generics.Count == 0) fbody.SpecializationMapper[new GenericsMapper()] = new TypeMapper();
        FunctionBodyDefinition(fbody, lambda.Statements);
        return fbody;
    }

    public static AnonymousFunctionBody MakeAnonymousFunction(INamespace ns)
    {
        var root = Lookup.GetRootNamespace(ns);
        var body = new AnonymousFunctionBody(ns, $"anonymous#{root.AnonymousFunctionUniqueCount++}");
        root.Structs.Add(body);
        root.Functions.Add(body);
        return body;
    }
}