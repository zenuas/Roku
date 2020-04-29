namespace Roku.Parser
{
    public interface IParser<T>
    {
        public void SyntaxError(T x) { }
    }
}
