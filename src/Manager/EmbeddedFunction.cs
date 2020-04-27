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
        public EmbeddedTokenValue? Return { get; set; } = null;
        public List<EmbeddedTokenValue> Arguments { get; } = new List<EmbeddedTokenValue>();
        public Func<string> OpCode { get; set; } = () => "";
        public Func<Assembly?> Assembly { get; set; } = () => null;

        public EmbeddedFunction(string name, string? ret, params string[] args)
        {
            Name = name;
            if (ret is { }) Return = new EmbeddedTokenValue(ret);
            args.Each(x => Arguments.Add(new EmbeddedTokenValue(x)));
        }
    }
}
