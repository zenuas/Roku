namespace Roku.IntermediateCode
{
    public class LabelCode : IOperand
    {
        public Operator Operator { get; set; } = Operator.Label;
    }
}
