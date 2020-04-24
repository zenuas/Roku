using Extensions;
using Roku.Compiler;
using Roku.IntermediateCode;
using Roku.Manager;
using Roku.Node;
using System;
using System.Collections.Generic;

namespace Roku
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var pgm = new ProgramNode() { FileName = "Sample.rk" };
            var call = new FunctionCallNode(new VariableNode() { Name = "fn" });
            call.Arguments.Add(new StringNode() { Value = "hello world" });
            pgm.Statements.Add(call);

            var fn = new FunctionNode(new VariableNode() { Name = "fn" });
            fn.Arguments.Add(new DeclareNode(new VariableNode() { Name = "x" }, new VariableNode() { Name = "String" }));
            var fn_call = new FunctionCallNode(new VariableNode() { Name = "print" });
            fn_call.Arguments.Add(new VariableNode() { Name = "x" });
            fn.Statements.Add(fn_call);
            pgm.Functions.Add(fn);

            var root = new RootNamespace();
            Lookup.LoadType(root, typeof(string));
            root.Functions.Add(new ExternFunction("print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) })!));

            var src = Definition.LoadProgram(root, pgm);
            src.Uses.Add(root);
            Typing.TypeInference(src);
            CodeGenerator.Emit(src, "a.il");
        }
    }
}
