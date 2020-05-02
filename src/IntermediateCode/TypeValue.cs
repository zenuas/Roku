namespace Roku.IntermediateCode
{
    public class TypeValue : ITypedValue
    {
        public string Name { get; }
        public Types Types { get; set; } = Types.Struct;

        public TypeValue(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;
    }
}
