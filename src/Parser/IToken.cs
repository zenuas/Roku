namespace Roku.Parser;

public interface IToken<T>
{
    public T Value { get; init; }
    public Symbols Symbol { get; init; }
}
