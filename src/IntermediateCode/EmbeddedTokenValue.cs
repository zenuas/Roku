namespace Roku.IntermediateCode
{
    public class EmbeddedTokenValue : ITypedValue
    {
        public string Name { get; }

        public EmbeddedTokenValue(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;
    }
}
