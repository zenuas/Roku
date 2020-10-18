namespace Roku.Manager
{
    public class FunctionSpecialization : IGenericsMapper
    {
        public IFunctionBody Body { get; }
        public GenericsMapper GenericsMapper { get; }

        public FunctionSpecialization(IFunctionBody body, GenericsMapper gen_map)
        {
            Body = body;
            GenericsMapper = gen_map;
        }
    }
}
