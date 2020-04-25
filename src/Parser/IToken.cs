namespace Roku.Parser
{
    public interface IToken<T> where T : class
    {
        public bool IsAccept { get; }
        public int LineNumber { get; set; }
        public int LineColumn { get; set; }
        public T? Value { get; set; }
        public int TableIndex { get; set; }
        public int InputToken { get; }
    }
}
