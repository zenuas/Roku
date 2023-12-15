﻿using Extensions;
using Roku.Declare;
using Roku.IntermediateCode;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Manager;

public class FunctionBody : IFunctionBody, ILexicalScope, IConstraints
{
    public required string Name { get; init; }
    public ITypeDefinition? Return { get; set; } = null;
    public List<(VariableValue Name, ITypeDefinition Type)> Arguments { get; } = new List<(VariableValue, ITypeDefinition)>();
    public List<IOperand> Body { get; } = [];
    public required IManaged Namespace { get; init; }
    public ILexicalScope? Parent { get; init; }
    public Dictionary<string, IEvaluable> LexicalScope { get; } = [];
    public int MaxTemporaryValue { get; set; } = 0;
    public List<TypeGenericsParameter> Generics { get; } = [];
    public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; } = [];
    public List<(VariableValue Class, List<ITypeDefinition> Generics)> Constraints { get; } = new List<(VariableValue, List<ITypeDefinition>)>();
    public Dictionary<VariableValue, ILexicalScope> Capture { get; } = [];

    public override string ToString() => $"sub {Name}({Arguments.Select(x => x.Name.ToString() + " : " + x.Type.ToString()).Join(", ")}){(Return is null ? "" : " " + Return.ToString())}";
}
