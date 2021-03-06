﻿using Extensions;
using System.Collections.Generic;

namespace Roku.Declare
{
    public class TypeSpecialization : ITypeDefinition
    {
        public string Name { get => Type.ToString()!; }
        public IEvaluable Type { get; }
        public List<ITypeDefinition> Generics { get; } = new List<ITypeDefinition>();

        public TypeSpecialization(IEvaluable type)
        {
            Type = type;
        }

        public override string ToString() => $"{Type}<{Generics.Map(x => x.ToString()!).Join(", ")}>";
    }
}
