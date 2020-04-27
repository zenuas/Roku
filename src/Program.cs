using Extensions;
using Roku.Compiler;
using Roku.Manager;
using Roku.Node;
using Roku.Parser;
using System;
using System.IO;
using System.Reflection;

namespace Roku
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var lex = new Lexer(new SourceCodeReader(new StringReader(@"
print(fn(""hello""))
print(fn(12))
print(1+2)
print(1-2)
print(2*3)
print(4/2)
print(5%3)
print(1<2)
print(1<=2)
print(2<=2)
print(3>2)
print(3>=2)
print(3>=3)
print(1==1)
print(1!=0)

sub fn(s: Int) Int
    var x = s + s
    return(x)

sub fn(s: String) String
    return(s + s)

#sub fn(s: a) a
#    return(s + s)
")));
            var pgm = new Parser.Parser().Parse(lex).Cast<ProgramNode>();

            var root = new RootNamespace();
            Lookup.LoadType(root, "String", typeof(string));
            Lookup.LoadType(root, "Int", typeof(int));
            Lookup.LoadType(root, "Bool", typeof(bool));
            root.Functions.Add(new ExternFunction("print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) })!));
            root.Functions.Add(new ExternFunction("print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(int) })!));
            root.Functions.Add(new ExternFunction("print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(bool) })!));
            //root.Functions.Add(new ExternFunction("+", typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) })!));
            root.Functions.Add(new EmbeddedFunction("+", "Int", "Int", "Int") { OpCode = (args) => $"{args[0]}\n{args[1]}\nadd" });
            root.Functions.Add(new EmbeddedFunction("-", "Int", "Int", "Int") { OpCode = (args) => $"{args[0]}\n{args[1]}\nsub" });
            root.Functions.Add(new EmbeddedFunction("*", "Int", "Int", "Int") { OpCode = (args) => $"{args[0]}\n{args[1]}\nmul" });
            root.Functions.Add(new EmbeddedFunction("/", "Int", "Int", "Int") { OpCode = (args) => $"{args[0]}\n{args[1]}\ndiv" });
            root.Functions.Add(new EmbeddedFunction("%", "Int", "Int", "Int") { OpCode = (args) => $"{args[0]}\n{args[1]}\nrem" });
            root.Functions.Add(new EmbeddedFunction("==", "Bool", "Int", "Int") { OpCode = (args) => $"{args[0]}\n{args[1]}\nceq" });
            root.Functions.Add(new EmbeddedFunction("!=", "Bool", "Int", "Int") { OpCode = (args) => $"{args[0]}\n{args[1]}\nceq\nldc.i4.0\nceq" });
            root.Functions.Add(new EmbeddedFunction("<", "Bool", "Int", "Int") { OpCode = (args) => $"{args[0]}\n{args[1]}\nclt" });
            root.Functions.Add(new EmbeddedFunction("<=", "Bool", "Int", "Int") { OpCode = (args) => $"{args[0]}\n{args[1]}\ncgt\nldc.i4.0\nceq" });
            root.Functions.Add(new EmbeddedFunction(">", "Bool", "Int", "Int") { OpCode = (args) => $"{args[0]}\n{args[1]}\ncgt" });
            root.Functions.Add(new EmbeddedFunction(">=", "Bool", "Int", "Int") { OpCode = (args) => $"{args[0]}\n{args[1]}\nclt\nldc.i4.0\nceq" });
            root.Functions.Add(new EmbeddedFunction("-", "Int", "Int") { OpCode = (args) => $"ldc.i4.0\n{args[0]}\nsub" });
            root.Functions.Add(new EmbeddedFunction("+", "String", "String", "String") { OpCode = (args) => $"{args[0]}\n{args[1]}\ncall string [System.Runtime]System.String::Concat(string, string)", Assembly = () => Assembly.Load("System.Runtime") });

            var src = Definition.LoadProgram(root, pgm);
            src.Uses.Add(root);
            Typing.TypeInference(src);
            CodeGenerator.Emit(src, "a.il");
        }
    }
}
