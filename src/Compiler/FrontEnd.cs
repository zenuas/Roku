using Extensions;
using Roku.Manager;
using Roku.Node;
using Roku.Parser;
using System;
using System.IO;
using System.Reflection;

namespace Roku.Compiler
{
    public static class FrontEnd
    {
        public static void Compile(string input, string output, string[] asms)
        {
            using var source = new StreamReader(input);
            Compile(source, output, asms);
        }

        public static void Compile(TextReader input, string output, string[] asms)
        {
            var lex = new Lexer(new SourceCodeReader(input));
            var pgm = new Parser.Parser().Parse(lex).Cast<ProgramNode>();

            var root = new RootNamespace();
            root.Assemblies.AddRange(asms.Map(Assembly.Load));
            _ = Lookup.LoadType(root, "String", typeof(string));
            _ = Lookup.LoadType(root, "Int", typeof(int));
            _ = Lookup.LoadType(root, "Long", typeof(long));
            _ = Lookup.LoadType(root, "Short", typeof(short));
            _ = Lookup.LoadType(root, "Byte", typeof(byte));
            _ = Lookup.LoadType(root, "Bool", typeof(bool));
            _ = Lookup.LoadType(root, "Object", typeof(object));
            _ = Lookup.LoadType(root, "List", typeof(System.Collections.Generic.List<>));
            DefineNumericFunction(root, "Int");
            DefineNumericFunction(root, "Long");
            DefineNumericFunction(root, "Short");
            DefineNumericFunction(root, "Byte");
            _ = Lookup.LoadFunction(root, "print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) })!);
            _ = Lookup.LoadFunction(root, "print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(int) })!);
            _ = Lookup.LoadFunction(root, "print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(long) })!);
            _ = Lookup.LoadFunction(root, "print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(byte) })!);
            _ = Lookup.LoadFunction(root, "print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(bool) })!);
            _ = Lookup.LoadFunction(root, "+", typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) })!);

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
