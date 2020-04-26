namespace Roku.IntermediateCode
{
    public class NumericValue : ITypedValue
    {
        public uint Value { get; }

        public NumericValue(uint n)
        {
            Value = n;
        }

        public override string ToString() => Value.ToString();
    }
}
