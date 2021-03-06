﻿namespace Roku.IntermediateCode
{
    public class GotoCode : IOperand
    {
        public Operator Operator { get; set; } = Operator.Goto;
        public LabelCode Label { get; set; }

        public GotoCode(LabelCode label)
        {
            Label = label;
        }

        public override string ToString() => $"goto {Label}";
    }
}
