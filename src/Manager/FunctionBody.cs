using Extensions;
using Roku.Declare;
using Roku.IntermediateCode;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Manager;

public class FunctionBody(IManaged ns, string name, ILexicalScope? parent = null) : IFunctionBody, ILexicalScope, IConstraints
{
    public string Name { get; } = name;
    public ITypeDefinition? Return { get; set; } = null;
    public List<(VariableValue Name, ITypeDefinition Type)> Arguments { get; } = new List<(VariableValue, ITypeDefinition)>();
    public List<IOperand> Body { get; } = [];
    public IManaged Namespace { get; } = ns;
    public ILexicalScope? Parent { get; } = parent;
    public Dictionary<string, IEvaluable> LexicalScope { get; } = [];
    public int MaxTemporaryValue { get; set; } = 0;
    public List<TypeGenericsParameter> Generics { get; } = [];
    public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; } = [];
    public List<(VariableValue Class, List<ITypeDefinition> Generics)> Constraints { get; } = new List<(VariableValue, List<ITypeDefinition>)>();
    public Dictionary<VariableValue, ILexicalScope> Capture { get; } = [];

    public override string ToString() => $"sub {Name}({Arguments.Select(x => x.Name.ToString() + " : " + x.Type.ToString()).Join(", ")}){(Return is null ? "" : " " + Return.ToString())}";
}
