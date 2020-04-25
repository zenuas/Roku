using Roku.Node;

namespace Roku.Parser
{
    public class Token
    {
        public string Name { get; set; } = "";
        public Symbols Type { get; set; }
        public int LineNumber { get; set; }
        public int LineColumn { get; set; }
        public int Indent { get; set; }
        public INode? Value { get; set; }
    }
}
