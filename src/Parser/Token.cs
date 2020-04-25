

using Roku.Node;



namespace Roku.Parser
{
    public class Token : IToken<INode>
    {
        public string Name { get; set; } = "";
        public Symbols Type { get; set; }
        public int LineNumber { get; set; }
        public int LineColumn { get; set; }
        public int Indent { get; set; }
        public INode? Value { get; set; }
        public int TableIndex { get; set; }
        public int InputToken => (int)Type;
        public bool IsAccept => Type == Symbols._END;
        public override string ToString() => Type.ToString();
    }
}
