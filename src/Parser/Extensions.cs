using Extensions;
using Roku.Node;

namespace Roku.Parser
{
    public static class Extensions
    {
        public static T R<T>(this T self, Token t) where T : INode => self.Return(x => { self.LineNumber = t.LineNumber; self.LineColumn = t.LineColumn; });

        public static T R<T>(this T self, INode t) where T : INode => self.Return(x => { self.LineNumber = t.LineNumber; self.LineColumn = t.LineColumn; });
    }
}
