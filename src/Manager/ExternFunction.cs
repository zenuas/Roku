using Roku.TypeSystem;

namespace Roku.Manager
{
    public class ExternFunction : IFunctionBody
    {
        public IFunction Function { get; set; }

        public ExternFunction(IFunction f)
        {
            Function = f;
        }
    }
}
