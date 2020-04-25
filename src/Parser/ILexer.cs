namespace Roku.Parser
{
    public interface ILexer<T> where T : class
    {
        public IToken<T> PeekToken();

        public IToken<T> ReadToken();
    }
}
