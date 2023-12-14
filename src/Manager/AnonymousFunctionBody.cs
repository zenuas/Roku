using Roku.Declare;
using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager;

public class AnonymousFunctionBody : IFunctionBody, IStructBody, ILexicalScope
{
    public string Name { get; }
    public ITypeDefinition? Return { get; set; } = null;
    public List<(VariableValue Name, ITypeDefinition Type)> Arguments { get; } = new List<(VariableValue, ITypeDefinition)>();
    public bool IsImplicit { get; set; } = true;
    public List<IOperand> Body { get; } = [];
    public IManaged Namespace { get; }
    public ILexicalScope? Parent { get; set; } = null;
    public Dictionary<string, IEvaluable> LexicalScope { get; } = [];
    public int MaxTemporaryValue { get; set; } = 0;
    public List<TypeGenericsParameter> Generics { get; } = [];
    public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; } = [];

    public AnonymousFunctionBody(IManaged ns, string system_unique_name, ILexicalScope parent)
    {
        Namespace = ns;
        Name = system_unique_name;
        Parent = parent;
    }
}
