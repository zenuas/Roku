namespace Roku.Node
{
    public class LetIgnoreNode : INode, ITupleBind
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
    }
}
