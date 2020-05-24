using System.Collections.Generic;

namespace Roku.Manager
{
    public interface IUse
    {
        public List<INamespace> Uses { get; }
    }
}
