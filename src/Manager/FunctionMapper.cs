namespace Roku.Manager
{
    public class FunctionMapper : IStructBody
    {
        public IFunctionBody Function { get; }
        public string Name { get => Function.Name; }
        public TypeMapper TypeMapper { get; } = new TypeMapper();

        public FunctionMapper(IFunctionBody f)
        {
            Function = f;
        }

        public override string ToString() => Function.ToString()!;
    }
}
