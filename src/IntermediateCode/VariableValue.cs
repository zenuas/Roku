namespace Roku.IntermediateCode
{
    public class VariableValue : ITypedValue
    {
        public string Name { get; }

        public VariableValue(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;
    }
}
