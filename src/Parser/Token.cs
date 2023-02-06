namespace Roku.Parser;

public class Token<T> : IToken<T>
{
    public required T Value { get; init; }
    public required Symbols Symbol { get; init; }
}
