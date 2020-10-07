namespace Roku.Declare
{
    public class StringValue : IEvaluable
    {
        public string Value { get; }

        public StringValue(string s)
        {
            Value = s;
        }

        public override string ToString() => $"\"{Value}\"";
    }
}
