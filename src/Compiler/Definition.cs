using Extensions;
using Roku.IntermediateCode;
using Roku.Manager;
using Roku.Node;
using Roku.TypeSystem;
using System;
using System.Collections.Generic;

namespace Roku.Compiler
{
    public static class Definition
    {
        public static void NamespaceDefinition(RootNamespace root, ProgramNode pgm)
        {
            _ = MakeNamespace(root, pgm);
        }

        public static SourceCodeBody LoadProgram(RootNamespace root, ProgramNode pgm)
        {
            var src = new SourceCodeBody(MakeNamespace(root, pgm));
            TypeDefinition(root, src, pgm);
            FunctionDefinition(root, src, pgm);
            return src;
        }

        public static void TypeDefinition(RootNamespace root, SourceCodeBody src, ProgramNode pgm)
        {
        }

        public static NamespaceManager MakeNamespace(RootNamespace root, ProgramNode pgm)
        {
            return pgm.FileName.Split(".").FoldLeft((ns, name) => MakeNamespace(ns, name), (NamespaceManager)root);
        }

        public static NamespaceManager MakeNamespace(NamespaceManager parent, string name)
        {
            var ns = parent.SubNamaspaces.FindFirstOrNull(x => x.Name == name);
            if (ns is { }) return ns;

            var n = new NamespaceManager(name);
            parent.SubNamaspaces.Add(n);
            return n;
        }

        public static void FunctionDefinition(RootNamespace root, SourceCodeBody src, ProgramNode pgm)
        {
            if (pgm.Statements.Count > 0)
            {
                var f = MakeEntrypoint(src.Current);
                FunctionBodyDefinition(root, pgm, f, pgm.Statements);
            }
        }

        public static void FunctionBodyDefinition(RootNamespace root, ProgramNode pgm, FunctionBody body, List<IStatementNode> stmts)
        {
            stmts.Each(stmt =>
                {
                    switch (stmt)
                    {
                        case FunctionCallNode call:
                            Call x;
                            if (call.Expression is PropertyNode prop)
                            {
                                x = new Call(prop.Right.Name) { FirstLookup = ToTypedValue(root, pgm, prop.Left) };
                            }
                            else
                            {
                                x = new Call(((VariableNode)call.Expression).Name);
                            }
                            call.Arguments.Each(arg => x.Arguments.Add(ToTypedValue(root, pgm, arg)));
                            body.Body.Add(x);
                            break;

                        default:
                            throw new Exception();
                    }
                });
        }

        public static IType LoadStruct(RootNamespace root, ProgramNode pgm, string name)
        {
            var current = MakeNamespace(root, pgm);
            var xs = LoadStructs(current, name);
            if (!xs.IsNull()) return xs.First();

            //pgm.Uses

            xs = LoadStructs(root, name);
            if (!xs.IsNull()) return xs.First();

            throw new Exception();
        }

        public static IEnumerable<IType> LoadStructs(NamespaceManager ns, string name)
        {
            return ns.Structs.Map(x => x.Struct).Where(x => x.Name == name);
        }

        public static ITypedValue ToTypedValue(RootNamespace root, ProgramNode pgm, IEvaluableNode e)
        {
            return e switch
            {
                StringNode x => new StringValue(x.Value) { Type = LoadStruct(root, pgm, "String") },
                _ => throw new Exception(),
            };
        }

        public static FunctionBody MakeEntrypoint(INamespace root)
        {
            var entry = root.Functions.FindFirstOrNull(x => x.Function.Name == "main");
            if (entry is FunctionBody main) return main;

            var f = new RkFunction("main");
            var body = new FunctionBody(f);
            root.Functions.Add(body);
            return body;
        }
    }
}
