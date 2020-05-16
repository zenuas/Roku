namespace Roku.IntermediateCode
{
    public class PropertyValue : ITypedValue
    {
        public ITypedValue Left { get; }
        public string Right { get; }

        public PropertyValue(ITypedValue left, string right)
        {
            Left = left;
            Right = right;
        }

        public override string ToString() => $"{Left}.{Right}";
    }
}
