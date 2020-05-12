using System.Reflection;

namespace Roku.Manager
{
    public class ExternStruct : IStructBody
    {
        public string Name { get; }
        public TypeInfo Struct { get; }
        public Assembly Assembly { get; }

        public ExternStruct(string name, TypeInfo f, Assembly asm)
        {
            Name = name;
            Struct = f;
            Assembly = asm;
        }

        public override string ToString() => Name;
    }
}
