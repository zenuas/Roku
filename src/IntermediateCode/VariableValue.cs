using Roku.Manager;

namespace Roku.IntermediateCode
{
    public class VariableValue : ITypedValue
    {
        public string Name { get; }
        public ILexicalScope Scope { get; }

        public VariableValue(string name, ILexicalScope scope)
        {
            Name = name;
            Scope = scope;
        }
    }
}
