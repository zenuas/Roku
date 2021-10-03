using Roku.Declare;
using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager
{
    public interface ILexicalScope : IAttachedNamespace
    {
        public ILexicalScope? Parent { get; }
        public Dictionary<string, IEvaluable> LexicalScope { get; }
        public List<IOperand> Body { get; }
        public int MaxTemporaryValue { get; set; }
    }
}
