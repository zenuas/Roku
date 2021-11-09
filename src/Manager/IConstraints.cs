using Roku.Declare;
using System.Collections.Generic;

namespace Roku.Manager;

public interface IConstraints
{
    public List<(VariableValue Class, List<ITypeDefinition> Generics)> Constraints { get; }
}
