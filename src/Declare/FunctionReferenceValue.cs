namespace Roku.Declare
{
    public class FunctionReferenceValue : IEvaluable
    {
        public IEvaluable Function { get; }

        public FunctionReferenceValue(IEvaluable f)
        {
            Function = f;
        }

        public override string ToString() => $"{Function}";
    }
}
