using Roku.Manager;
using Roku.TypeSystem;

namespace Roku.IntermediateCode
{
    public class VariableValue : ITypedValue
    {
        public string Name { get; set; }
        public IType? Type { get; set; }
        public IScope Scope { get; }

        public VariableValue(string name, IScope scope)
        {
            Name = name;
            Scope = scope;
        }
    }
}
