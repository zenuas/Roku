using Roku.Manager;
using Roku.TypeSystem;

namespace Roku.IntermediateCode
{
    public class VariableValue : ITypedValue
    {
        public string Name { get; set; }
        public IType? Type { get; set; }
        public ILexicalScope Scope { get; }

        public VariableValue(string name, ILexicalScope scope)
        {
            Name = name;
            Scope = scope;
        }
    }
}
