using Roku.Declare;
using Roku.IntermediateCode;
using System;
using System.Collections.Generic;

namespace Roku.Manager;

public class InstanceBody : INamespace, ILexicalScope
{
    public IManaged Namespace { get; }
    public ITypeDefinition Type { get; set; }
    public TypeSpecialization Specialization { get; set; }
    public List<IFunctionName> Functions { get; } = [];
    public List<IStructBody> Structs { get; } = [];
    public List<ClassBody> Classes { get; } = [];
    public List<InstanceBody> Instances => throw new NotImplementedException();
    public ILexicalScope? Parent { get; } = null;
    public Dictionary<string, IEvaluable> LexicalScope { get; } = [];
    public List<IOperand> Body { get; } = [];
    public int MaxTemporaryValue { get; set; } = 0;

#pragma warning disable CS8618
    public InstanceBody(IManaged ns)
    {
        Namespace = ns;
    }
#pragma warning restore CS8618
}
