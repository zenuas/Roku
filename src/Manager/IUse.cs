using System.Collections.Generic;

namespace Roku.Manager;

public interface IUse
{
    public List<IManaged> Uses { get; }
}
