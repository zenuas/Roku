﻿using Roku.Declare;
using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class StructBody : IStructBody, ILexicalScope, ISpecialization
    {
        public string Name { get; }
        public Dictionary<string, IEvaluable> Members { get; } = new Dictionary<string, IEvaluable>();
        public List<IOperand> Body { get; } = new List<IOperand>();
        public INamespace Namespace { get; }
        public ILexicalScope? Parent { get; } = null;
        public Dictionary<string, IEvaluable> LexicalScope { get; } = new Dictionary<string, IEvaluable>();
        public int MaxTemporaryValue { get; set; } = 0;
        public List<TypeGenericsParameter> Generics { get; } = new List<TypeGenericsParameter>();
        public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; } = new Dictionary<GenericsMapper, TypeMapper>();

        public StructBody(INamespace ns, string name)
        {
            Namespace = ns;
            Name = name;
        }

        public override string ToString() => Name;
    }
}
