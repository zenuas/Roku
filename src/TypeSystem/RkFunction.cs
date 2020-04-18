using Extensions;
using System.Collections.Generic;

namespace Roku.TypeSystem
{
    public class RkFunction : IFunction
    {
        public string Name { get; }
        public IType? Return { get; set; } = null;
        public List<(string Name, IType Value)> NamedArguments { get; } = new List<(string Name, IType Value)>();
        public List<IType> Arguments => NamedArguments.UnZip().Second.ToList();

        public RkFunction(string name)
        {
            Name = name;
        }
    }
}
