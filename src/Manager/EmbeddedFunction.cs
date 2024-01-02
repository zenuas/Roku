using Mina.Extensions;
using Roku.Declare;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Manager;

public class EmbeddedFunction(string name) : IFunctionName, IFunctionReturn, ISpecialization
{
    public string Name { get; init; } = name;
    public ITypeDefinition? Return { get; set; } = null;
    public List<ITypeDefinition> Arguments { get; init; } = [];
    public Func<FunctionMapper, string[], string> OpCode { get; set; } = (_, _) => "";
    public List<TypeGenericsParameter> Generics { get; init; } = [];
    public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; init; } = [];

    public EmbeddedFunction(string name, string? ret, params string[] args) : this(name, ret, args.Select(x => new TypeValue { Name = x }).ToArray())
    {
    }

    public EmbeddedFunction(string name, string? ret, ITypeDefinition[] args) : this(name)
    {
        if (ret is { }) Return = new TypeValue { Name = ret };
        Arguments.AddRange(args);
    }

    public override string ToString() => $"sub {Name}({Arguments.Select(x => x.ToString()!).Join(", ")}){(Return is { } ? " " + Return.ToString() : "")}";
}
