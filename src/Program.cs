﻿using Extensions;
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

sub fn(s: Int) Int
    return(s + s)

sub fn(s: String) String
    return(s + s)

#sub fn(s: a) a
#    return(s + s)
")));
            var pgm = new Parser.Parser().Parse(lex).Cast<ProgramNode>();

            var root = new RootNamespace();
            Lookup.LoadType(root, "String", typeof(string));
            Lookup.LoadType(root, "Int", typeof(int));
            root.Functions.Add(new ExternFunction("print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) })!));
            root.Functions.Add(new ExternFunction("print", typeof(Console).GetMethod("WriteLine", new Type[] { typeof(int) })!));
            //root.Functions.Add(new ExternFunction("+", typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) })!));
            root.Functions.Add(new EmbeddedFunction("+", "Int", "Int", "Int") { OpCode = () => "add" });
            root.Functions.Add(new EmbeddedFunction("-", "Int", "Int", "Int") { OpCode = () => "sub" });
            root.Functions.Add(new EmbeddedFunction("+", "String", "String", "String") { OpCode = () => "call string [System.Runtime]System.String::Concat(string, string)", Assembly = () => Assembly.Load("System.Runtime") });

            var src = Definition.LoadProgram(root, pgm);
            src.Uses.Add(root);
            Typing.TypeInference(src);
            CodeGenerator.Emit(src, "a.il");
        }
    }
}
