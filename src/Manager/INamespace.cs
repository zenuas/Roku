﻿using System.Collections.Generic;

namespace Roku.Manager
{
    public interface INamespace
    {
        public List<IFunctionBody> Functions { get; }
        public List<StructBody> Structs { get; }
    }
}
