using Roku.Declare;
using Roku.IntermediateCode;
using System;
using System.Collections.Generic;

namespace Roku.Manager;

public class InstanceBody : INamespaceBody, ILexicalScope
{
    public INamespace Namespace { get; }
    public ITypeDefinition Type { get; set; }
    public TypeSpecialization Specialization { get; set; }
    public List<IFunctionName> Functions { get; } = new List<IFunctionName>();
    public List<IStructBody> Structs { get; } = new List<IStructBody>();
    public List<ClassBody> Classes { get; } = new List<ClassBody>();
    public List<InstanceBody> Instances => throw new NotImplementedException();
    public ILexicalScope? Parent { get; } = null;
    public Dictionary<string, IEvaluable> LexicalScope { get; } = new Dictionary<string, IEvaluable>();
    public List<IOperand> Body { get; } = new List<IOperand>();
    public int MaxTemporaryValue { get; set; } = 0;

#pragma warning disable CS8618
    public InstanceBody(INamespace ns)
    {
        Namespace = ns;
    }
#pragma warning restore CS8618
}
