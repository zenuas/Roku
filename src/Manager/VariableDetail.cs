﻿using Roku.IntermediateCode;

namespace Roku.Manager
{
    public class VariableDetail
    {
        public string Name { get; set; } = "";
        public IStructBody? Struct { get; set; }
        public ITypedValue? Reciever { get; set; }
        public VariableType Type { get; set; }
        public int Index { get; set; }

        public override string ToString() => (Reciever is null ? "" : $"{Reciever}.") + Struct?.ToString() ?? "";
    }
}
