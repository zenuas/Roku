namespace Roku.Parser;

public interface IToken<T>
{
    public T Value { get; }
    public Symbols Symbol { get; }
}
