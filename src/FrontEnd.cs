using Command;
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
    public static class FrontEnd
    {
        public static void Main(string[] args)
        {
            var opt = new Option();
#if DEBUG
            opt.Output = "-";
#endif
            var xs = CommandLine.Run(opt, args);
            if (xs.Length == 0)
            {
                Compile(Console.In, opt.Output);
            }
            else
            {
                Compile(xs[0], opt.Output);
            }
        }

        public static void Compile(string input, string output)
        {
            using var source = new StreamReader(input);
            Compile(source, output);
        }

        public static void Compile(TextReader input, string output)
        {
            var lex = new Lexer(new SourceCodeReader(input));
            var pgm = new Parser.Parser().Parse(lex).Cast<ProgramNode>();

            var root = new RootNamespace();
            _ = Lookup.LoadType(root, "String", typeof(string));
            _ = Lookup.LoadType(root, "Int", typeof(int));
            _ = Lookup.LoadType(root, "Long", typeof(long));
            _ = Lookup.LoadType(root, "Short", typeof(short));
            _ = Lookup.LoadType(root, "Byte", typeof(byte));
            _ = Lookup.LoadType(root, "Bool", typeof(bool));
            DefineNumericFunction(root, "Int");
            DefineNumericFunction(root, "Long");
            DefineNumericFunction(root, "Short");
            DefineNumericFunction(root, "Byte");
            root.Functions.Add(new ExternFunction("print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) })!));
            root.Functions.Add(new ExternFunction("print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(int) })!));
            root.Functions.Add(new ExternFunction("print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(long) })!));
            root.Functions.Add(new ExternFunction("print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(byte) })!));
            root.Functions.Add(new ExternFunction("print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(bool) })!));
            root.Functions.Add(new ExternFunction("+", typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) })!) { Assembly = Assembly.Load("System.Runtime") });
            root.Functions.Add(new EmbeddedFunction("#is_not_null", "Bool", "a") { OpCode = (args) => $"{args[0]}\nldnull\ncgt.un" });

            var src = Definition.LoadProgram(root, pgm);
            src.Uses.Add(root);
            Typing.TypeInference(src);
            CodeGenerator.Emit(src, output);
        }

        public static void DefineNumericFunction(RootNamespace root, string type)
        {
            root.Functions.Add(new EmbeddedFunction("+", type, type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\nadd" });
            root.Functions.Add(new EmbeddedFunction("-", type, type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\nsub" });
            root.Functions.Add(new EmbeddedFunction("*", type, type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\nmul" });
            root.Functions.Add(new EmbeddedFunction("/", type, type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\ndiv" });
            root.Functions.Add(new EmbeddedFunction("%", type, type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\nrem" });
            root.Functions.Add(new EmbeddedFunction("==", "Bool", type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\nceq" });
            root.Functions.Add(new EmbeddedFunction("!=", "Bool", type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\nceq\nldc.i4.0\nceq" });
            root.Functions.Add(new EmbeddedFunction("<", "Bool", type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\nclt" });
            root.Functions.Add(new EmbeddedFunction("<=", "Bool", type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\ncgt\nldc.i4.0\nceq" });
            root.Functions.Add(new EmbeddedFunction(">", "Bool", type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\ncgt" });
            root.Functions.Add(new EmbeddedFunction(">=", "Bool", type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\nclt\nldc.i4.0\nceq" });
            root.Functions.Add(new EmbeddedFunction("-", type, type) { OpCode = (args) => $"ldc.i4.0\n{args[0]}\nsub" });
        }
    }
}
