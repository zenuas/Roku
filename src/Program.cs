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
fn(""hello"", 123_456)

sub fn(s: String, n: Int)
    var x = s + "" world"" + ""!!""
    var y = 234_567 - n
    print(x)
    print(n)
")));
            var pgm = new Parser.Parser().Parse(lex).Cast<ProgramNode>();

            var root = new RootNamespace();
            Lookup.LoadType(root, "String", typeof(string));
            Lookup.LoadType(root, "Int", typeof(int));
            root.Functions.Add(new ExternFunction("print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) })!));
            root.Functions.Add(new ExternFunction("print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(int) })!));

            var src = Definition.LoadProgram(root, pgm);
            src.Uses.Add(root);
            Typing.TypeInference(src);
            CodeGenerator.Emit(src, "a.il");
        }
    }
}
