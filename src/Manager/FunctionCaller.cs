namespace Roku.Manager
{
    public class FunctionCaller
    {
        public IFunctionBody Body { get; }
        public GenericsMapper GenericsMapper { get; }

        public FunctionCaller(IFunctionBody body, GenericsMapper gen_map)
        {
            Body = body;
            GenericsMapper = gen_map;
        }
    }
}
