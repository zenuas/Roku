using System;

namespace Roku.IntermediateCode
{
    public class TypeInfoValue : ITypeDefinition
    {
        public Type Type { get; set; }

        public TypeInfoValue(Type type)
        {
            Type = type;
        }

        public override string ToString() => Type.Name;
    }
}
