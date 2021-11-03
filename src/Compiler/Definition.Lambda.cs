using Extensions;
using Roku.Declare;
using Roku.Manager;
using Roku.Node;

namespace Roku.Compiler
{
    public static partial class Definition
    {
        public static AnonymousFunctionBody LambdaExpressionDefinition(ILexicalScope scope, LambdaExpressionNode lambda)
        {
            var fbody = MakeAnonymousFunction(scope.Namespace);
            fbody.IsImplicit = lambda.IsImplicit;
            if (lambda.Return is { } ret) fbody.Return = CreateType(scope, ret);
            if (fbody.IsImplicit) fbody.Return = new TypeImplicit();
            lambda.Arguments.Each(x =>
                {
                    ITypeDefinition p;
                    if (x is DeclareNode decla)
                    {
                        p = CreateType(scope, decla.Type);
                    }
                    else
                    {
                        p = new TypeGenericsParameter(x.Name.Name);
                        scope.LexicalScope[x.Name.Name] = p;
                    }
                    if (p is TypeGenericsParameter g) fbody.Generics.Add(g);
                    fbody.Arguments.Add((new VariableValue(x.Name.Name), p));
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
}
