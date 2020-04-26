namespace Roku.IntermediateCode
{
    public class StringValue : ITypedValue
    {
        public string Value { get; }

        public StringValue(string s)
        {
            Value = s;
        }

        public override string ToString() => $"\"{Value}\"";
    }
}
