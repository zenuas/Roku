using System.Collections.Generic;

namespace Roku.Manager
{
    public interface INamespace
    {
        public INamespace Current { get; }
        public List<IFunctionBody> Functions { get; }
    }
}
