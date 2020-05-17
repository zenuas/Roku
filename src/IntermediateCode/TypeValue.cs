﻿using Extensions;

namespace Roku.IntermediateCode
{
    public class TypeValue : ITypeDefinition
    {
        public string Name { get; }
        public Types Types { get; set; } = Types.Struct;

        public TypeValue(string name)
        {
            Name = name;
            Types = char.IsLower(name.First()) ? Types.Generics : Types.Struct;
        }

        public override string ToString() => Name;
    }
}
