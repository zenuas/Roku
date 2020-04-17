using System.Reflection;

namespace Roku.TypeSystem
{
    public class RkCILStruct : IType
    {
        public string Name { get; set; }
        public TypeInfo TypeInfo { get; set; }

        public RkCILStruct(string name, TypeInfo type)
        {
            Name = name;
            TypeInfo = type;
        }
    }
}
