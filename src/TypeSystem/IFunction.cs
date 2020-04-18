using System.Collections.Generic;

namespace Roku.TypeSystem
{
    public interface IFunction
    {
        public string Name { get; }
        public IType? Return { get; set; }
        public List<IType> Arguments { get; }
    }
}
