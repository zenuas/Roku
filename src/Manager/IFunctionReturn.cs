using Roku.Declare;

namespace Roku.Manager;

public interface IFunctionReturn
{
    public ITypeDefinition? Return { get; set; }
}
