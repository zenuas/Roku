using Roku.Manager;
using System.Linq;

namespace Roku.Compiler
{
    public static class Garbage
    {
        public static bool IncompleteType(StructBody sb)
        {
            foreach (var gm in sb.SpecializationMapper.Keys.ToArray())
            {
                if (!Lookup.IsFixedGenericsMapper(gm)) sb.SpecializationMapper.Remove(gm);
            }
            return sb.SpecializationMapper.Count > 0;
        }
    }
}
