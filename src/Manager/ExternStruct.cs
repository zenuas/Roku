using System.Reflection;

namespace Roku.Manager
{
    public class ExternStruct : IStructBody
    {
        public string Name { get; }
        public TypeInfo Struct { get; }
        public Assembly? Assembly { get; set; }

        public ExternStruct(string name, TypeInfo f)
        {
            Name = name;
            Struct = f;
        }

        public override string ToString() => Name;
    }
}
