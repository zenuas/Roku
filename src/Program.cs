using Extensions;
using Roku.Compiler;
using Roku.Manager;
using Roku.Node;
using Roku.Parser;
using System;
using System.IO;

namespace Roku
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var lex = new Lexer(new SourceCodeReader(new StringReader(@"
fn(""hello world"")

sub fn(s: String)
    print(s)
")));
            var pgm = new Parser.Parser().Parse(lex).Cast<ProgramNode>();

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
