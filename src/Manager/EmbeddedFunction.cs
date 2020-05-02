using Extensions;
using Roku.IntermediateCode;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Roku.Manager
{
    public class EmbeddedFunction : IFunctionBody
    {
        public string Name { get; }
        public TypeValue? Return { get; set; } = null;
        public List<TypeValue> Arguments { get; } = new List<TypeValue>();
        public Func<string[], string> OpCode { get; set; } = (xs) => "";
        public Func<Assembly?> Assembly { get; set; } = () => null;

        public EmbeddedFunction(string name, string? ret, params string[] args)
        {
            Name = name;
            if (ret is { }) Return = new TypeValue(ret);
            args.Each(x => Arguments.Add(new TypeValue(x)));
        }
    }
}
