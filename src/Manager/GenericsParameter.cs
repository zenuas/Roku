namespace Roku.Manager
{
    public class GenericsParameter : IStructBody
    {
        public string Name { get; }

        public GenericsParameter(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;
    }
}
