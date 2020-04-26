using Roku.Manager;

namespace Roku.IntermediateCode
{
    public class TemporaryValue : ITypedValue
    {
        public string Name { get; }
        public int Index { get; }
        public ILexicalScope Scope { get; }

        public TemporaryValue(string name, int index, ILexicalScope scope)
        {
            Name = name;
            Index = index;
            Scope = scope;
        }

        public override string ToString() => Name;
    }
}
