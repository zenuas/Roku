using Extensions;
using Roku.Declare;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class GenericsMapper : Dictionary<ITypeDefinition, IStructBody?>
    {
        public KeyValuePair<ITypeDefinition, IStructBody?> GetKeyValue(string name) => this.FindFirst(x => x.Key.Name == name);
        public IStructBody? GetValue(string name) => GetKeyValue(name).Value;
        public bool ContainsKey(string name) => this.FindFirstOrNullValue(x => x.Key.Name == name) is { };
    }
}
