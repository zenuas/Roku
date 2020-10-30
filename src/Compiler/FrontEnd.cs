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
            var par = new Parser.Parser();
            lex.Parser = par;
            var pgm = par.Parse(lex).Cast<ProgramNode>();

            var root = new RootNamespace();
            root.Assemblies.AddRange(asms.Map(Assembly.Load));
            Lookup.LoadType(root, typeof(string)).Name = "String";
            Lookup.LoadType(root, typeof(int)).Name = "Int";
            Lookup.LoadType(root, typeof(long)).Name = "Long";
            Lookup.LoadType(root, typeof(short)).Name = "Short";
            Lookup.LoadType(root, typeof(byte)).Name = "Byte";
            Lookup.LoadType(root, typeof(bool)).Name = "Bool";
            Lookup.LoadType(root, typeof(float)).Name = "Float";
            Lookup.LoadType(root, typeof(double)).Name = "Double";
            Lookup.LoadType(root, typeof(object)).Name = "Object";
            DefineNumericFunction(root, "Int");
            DefineNumericFunction(root, "Long");
            DefineNumericFunction(root, "Short");
            DefineNumericFunction(root, "Byte");
            DefineNumericFunction(root, "Float", "ldc.r4 0");
            DefineNumericFunction(root, "Double", "ldc.r8 0");
            _ = Lookup.LoadFunction(root, "print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) })!);
            _ = Lookup.LoadFunction(root, "print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(int) })!);
            _ = Lookup.LoadFunction(root, "print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(long) })!);
            _ = Lookup.LoadFunction(root, "print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(byte) })!);
            _ = Lookup.LoadFunction(root, "print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(bool) })!);
            _ = Lookup.LoadFunction(root, "print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(float) })!);
            _ = Lookup.LoadFunction(root, "print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(double) })!);
            _ = Lookup.LoadFunction(root, "+", typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) })!);

            var src = Definition.LoadProgram(root, pgm);
            Typing.TypeInference(root, src);
            CodeGenerator.Emit(root, src, output);
        }

        public static void DefineNumericFunction(RootNamespace root, string type, string zero = "ldc.i4.0")
        {
            root.Functions.Add(new EmbeddedFunction("+", type, type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\nadd" });
            root.Functions.Add(new EmbeddedFunction("-", type, type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\nsub" });
            root.Functions.Add(new EmbeddedFunction("*", type, type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\nmul" });
            root.Functions.Add(new EmbeddedFunction("/", type, type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\ndiv" });
            root.Functions.Add(new EmbeddedFunction("%", type, type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\nrem" });
            root.Functions.Add(new EmbeddedFunction("==", "Bool", type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\nceq" });
            root.Functions.Add(new EmbeddedFunction("!=", "Bool", type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\nceq\n{zero}\nceq" });
            root.Functions.Add(new EmbeddedFunction("<", "Bool", type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\nclt" });
            root.Functions.Add(new EmbeddedFunction("<=", "Bool", type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\ncgt\n{zero}\nceq" });
            root.Functions.Add(new EmbeddedFunction(">", "Bool", type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\ncgt" });
            root.Functions.Add(new EmbeddedFunction(">=", "Bool", type, type) { OpCode = (args) => $"{args[0]}\n{args[1]}\nclt\n{zero}\nceq" });
            root.Functions.Add(new EmbeddedFunction("+", type, type) { OpCode = (args) => args[0] });
            root.Functions.Add(new EmbeddedFunction("-", type, type) { OpCode = (args) => $"{args[0]}\nneg" });
        }
    }
}
