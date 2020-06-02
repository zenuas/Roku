using Extensions;
using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class GenericsMapper : Dictionary<TypeValue, IStructBody?>
    {
        public KeyValuePair<TypeValue, IStructBody?> GetKeyValue(string name) => this.FindFirst(x => x.Key.Name == name);
        public IStructBody? GetValue(string name) => GetKeyValue(name).Value;
        public bool ContainsKey(string name) => this.FindFirstOrNullValue(x => x.Key.Name == name) is { };
    }
}
