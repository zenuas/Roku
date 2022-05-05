using Roku.Declare;
using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager;

public interface IFunctionBody : IFunctionName, IFunctionReturn, IManaged, ISpecialization
{
    public List<(VariableValue Name, ITypeDefinition Type)> Arguments { get; }
    public List<IOperand> Body { get; }
    public IManaged Namespace { get; }
    public ILexicalScope? Parent { get; }
}
