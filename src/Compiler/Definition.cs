using Extensions;
using Roku.IntermediateCode;
using Roku.Manager;
using Roku.Node;
using System;
using System.Collections.Generic;

namespace Roku.Compiler
{
    public static class Definition
    {
        public static SourceCodeBody LoadProgram(RootNamespace root, ProgramNode pgm)
        {
            var src = new SourceCodeBody();
            src.Uses.Add(root);
            TypeDefinition(src, pgm);
            FunctionDefinition(src, pgm);
            return src;
        }

        public static void TypeDefinition(SourceCodeBody src, ProgramNode pgm)
        {
        }

        public static void FunctionDefinition(SourceCodeBody src, ProgramNode pgm)
        {
            if (pgm.Statements.Count > 0)
            {
                FunctionBodyDefinition(MakeFunction(src, "main"), pgm.Statements);
            }
            pgm.Functions.Each(f =>
                {
                    var body = MakeFunction(src, f.Name.Name);
                    f.Arguments.Each(x =>
                        {
                            //var t = Lookup.LoadStruct(src, x.Type.Name);
                            var name = new VariableValue(x.Name.Name, body);
                            body.Arguments.Add((name, new VariableValue(x.Type.Name, body)));
                            body.LexicalScope.Add(x.Name.Name, name);
                        });

                    FunctionBodyDefinition(body, f.Statements);
                });
        }

        public static void FunctionBodyDefinition(ILexicalScope scope, List<IStatementNode> stmts)
        {
            stmts.Each(stmt =>
            {
                switch (stmt)
                {
                    case FunctionCallNode call:
                        Call x;
                        if (call.Expression is PropertyNode prop)
                        {
                            x = new Call(new VariableValue(prop.Right.Name, scope)) { FirstLookup = ToTypedValue(scope, prop.Left) };
                        }
                        else
                        {
                            x = new Call(new VariableValue(call.Expression.Cast<VariableNode>().Name, scope));
                        }
                        call.Arguments.Each(arg => x.Arguments.Add(ToTypedValue(scope, arg)));
                        scope.Body.Add(x);
                        break;

                    default:
                        throw new Exception();
                }
            });
        }

        public static ITypedValue ToTypedValue(ILexicalScope scope, IEvaluableNode e)
        {
            switch (e)
            {
                case StringNode x:
                    return new StringValue(x.Value);

                case VariableNode x:
                    return FindScopeValue(scope, x.Name);

                default:
                    throw new Exception();
            }
        }

        public static ITypedValue FindScopeValue(ILexicalScope scope, string name)
        {
            return scope.LexicalScope.ContainsKey(name) ? scope.LexicalScope[name] : FindScopeValue(scope.Parent!, name);
        }

        public static FunctionBody MakeFunction(INamespace ns, string name)
        {
            var body = new FunctionBody(ns, name);
            ns.Functions.Add(body);
            return body;
        }
    }
}
