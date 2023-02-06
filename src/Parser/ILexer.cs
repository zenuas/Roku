namespace Roku.Parser;

public interface ILexer<T>
{
    public IToken<T> PeekToken();

    public IToken<T> ReadToken();
}
