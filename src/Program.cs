using Roku.Compiler;
using Roku.Manager;
using Roku.Node;
using System;

namespace Roku
{
    public static class Program
    {
        public static void Main(string[] args)
        {

            var pgm = new ProgramNode() { FileName = "Sample" };
            var v = new VariableNode() { Name = "print" };
            var a = new StringNode() { Value = "hello world" };
            var call = new FunctionCallNode(v);
            call.Arguments.Add(a);
            pgm.Statements.Add(call);

            var root = new RootNamespace();
            RootNamespace.LoadType(root, typeof(string));
            var print = RootNamespace.LoadFunction(root, "print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) })!);
            root.Functions.Add(new ExternFunction(print));

            Definition.NamespaceDefinition(root, pgm);
            var src = Definition.LoadProgram(root, pgm);
            src.Uses.Add(root);
            Typing.TypeInference(src);

            _ = Console.ReadKey();
        }
    }
}
