using Roku.TypeSystem;

namespace Roku.IntermediateCode
{
    public class StringValue : ITypedValue
    {
        public string Value { get; set; }
        public IType? Type { get; set; }

        public StringValue(string s)
        {
            Value = s;
        }
    }
}
