using Roku.Parser;

namespace Roku.Node;

public interface INode : IToken<INode>
{
    public int Indent { get; set; }
    int? LineNumber { get; set; }
    int? LineColumn { get; set; }
}
