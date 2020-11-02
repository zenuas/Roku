namespace Roku.Declare
{
    public class FunctionReferenceValue : IEvaluable
    {
        public string Name { get; }

        public FunctionReferenceValue(string name)
        {
            Name = name;
        }

        public override string ToString() => $"{Name}";
    }
}
