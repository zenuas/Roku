namespace Roku.Declare
{
    public class ImplicitReturnValue : IEvaluable
    {
        public string Name { get; } = "_";

        public override string ToString() => Name;
    }
}
