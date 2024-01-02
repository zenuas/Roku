using Mina.Extensions;
using Roku.Node;

namespace Roku.Parser;

public static class Extensions
{
    public static T R<T>(this T self, IToken<INode> t) where T : IToken<INode> => self.Return(x => { self.Value.LineNumber = t.Value.LineNumber; self.Value.LineColumn = t.Value.LineColumn; });
}
