using Roku.Declare;
using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager
{
    public interface IFunctionBody : IFunctionName, INamespace, ISpecialization
    {
        public ITypeDefinition? Return { get; set; }
        public List<(VariableValue Name, ITypeDefinition Type)> Arguments { get; }
        public List<IOperand> Body { get; }
        public INamespace Namespace { get; }
        public ILexicalScope? Parent { get; }
    }
}
