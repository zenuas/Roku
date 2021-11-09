using Extensions;
using Roku.Declare;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Manager;

public class EmbeddedFunction : IFunctionName, IFunctionReturn, ISpecialization
{
    public string Name { get; }
    public ITypeDefinition? Return { get; set; } = null;
    public List<ITypeDefinition> Arguments { get; } = new List<ITypeDefinition>();
    public Func<FunctionMapper, string[], string> OpCode { get; set; } = (_, _) => "";
    public List<TypeGenericsParameter> Generics { get; } = new List<TypeGenericsParameter>();
    public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; } = new Dictionary<GenericsMapper, TypeMapper>();

    public EmbeddedFunction(string name, string? ret, params string[] args) : this(name, ret, args.Select(x => new TypeValue() { Name = x }).ToArray())
    {
    }

    public EmbeddedFunction(string name, string? ret, ITypeDefinition[] args) : this(name)
    {
        if (ret is { }) Return = new TypeValue() { Name = ret };
        Arguments.AddRange(args);
    }

    public EmbeddedFunction(string name)
    {
        Name = name;
    }

    public override string ToString() => $"sub {Name}({Arguments.Select(x => x.ToString()!).Join(", ")}){(Return is { } ? " " + Return.ToString() : "") }";
}
