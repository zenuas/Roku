using Roku.Declare;

namespace Roku.IntermediateCode;

public interface IReturnBind
{
    public IEvaluable? Return { get; set; }
}
