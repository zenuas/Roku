using Extensions;
using Roku.Declare;
using System;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class EmbeddedFunction : IFunctionName, IFunctionReturn
    {
        public string Name { get; }
        public ITypeDefinition? Return { get; set; } = null;
        public List<ITypeDefinition> Arguments { get; } = new List<ITypeDefinition>();
        public Func<string[], string> OpCode { get; set; } = (xs) => "";

        public EmbeddedFunction(string name, string? ret, params string[] args) : this(name, ret, args.Map(x => new TypeValue(x)).ToArray())
        {
        }

        public EmbeddedFunction(string name, string? ret, ITypeDefinition[] args)
        {
            Name = name;
            if (ret is { }) Return = new TypeValue(ret);
            Arguments.AddRange(args);
        }

        public override string ToString() => $"sub {Name}({Arguments.Map(x => x.ToString()!).Join(", ")}){(Return is { } ? " " + Return.ToString() : "") }";
    }
}
