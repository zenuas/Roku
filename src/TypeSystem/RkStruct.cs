namespace Roku.TypeSystem
{
    public class RkStruct : IType
    {
        public string Name { get; set; }

        public RkStruct(string name)
        {
            Name = name;
        }
    }
}
