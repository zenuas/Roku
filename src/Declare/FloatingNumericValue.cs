namespace Roku.Declare
{
    public class FloatingNumericValue : IEvaluable
    {
        public double Value { get; }

        public FloatingNumericValue(double n)
        {
            Value = n;
        }

        public override string ToString() => Value.ToString();
    }
}
