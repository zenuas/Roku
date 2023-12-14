using Roku.Declare;
using Roku.IntermediateCode;
using System;
using System.Collections.Generic;

namespace Roku.Manager;

#pragma warning disable CS8618
public class InstanceBody(IManaged ns) : INamespace, ILexicalScope
#pragma warning restore CS8618
{
    public IManaged Namespace { get; } = ns;
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
}
