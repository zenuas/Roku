﻿using Extensions;

namespace Roku.IntermediateCode
{
    public class TypeValue : ITypeDefinition
    {
        public string Name { get; }

        public TypeValue(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;
    }
}
