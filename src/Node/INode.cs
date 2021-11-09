namespace Roku.Node;

public interface INode
{
    int? LineNumber { get; set; }
    int? LineColumn { get; set; }
}
