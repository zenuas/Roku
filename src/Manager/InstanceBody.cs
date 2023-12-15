using Roku.Declare;
using Roku.IntermediateCode;
using System;
using System.Collections.Generic;

namespace Roku.Manager;

public class InstanceBody : INamespace, ILexicalScope
{
    public required IManaged Namespace { get; init; }
    public ITypeDefinition Type { get; set; } = default!;
    public TypeSpecialization Specialization { get; set; } = default!;
    public List<IFunctionName> Functions { get; init; } = [];
    public List<IStructBody> Structs { get; init; } = [];
    public List<ClassBody> Classes { get; init; } = [];
    public List<InstanceBody> Instances => throw new NotImplementedException();
    public ILexicalScope? Parent { get; init; } = null;
    public Dictionary<string, IEvaluable> LexicalScope { get; init; } = [];
    public List<IOperand> Body { get; init; } = [];
    public int MaxTemporaryValue { get; set; } = 0;
}
