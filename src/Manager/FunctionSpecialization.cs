namespace Roku.Manager
{
    public class FunctionSpecialization : IGenericsMapper
    {
        public IFunctionName Body { get; }
        public GenericsMapper GenericsMapper { get; }

        public FunctionSpecialization(IFunctionName body, GenericsMapper gen_map)
        {
            Body = body;
            GenericsMapper = gen_map;
        }
    }
}
