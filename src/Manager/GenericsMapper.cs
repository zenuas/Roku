using Mina.Extension;
using Roku.Declare;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Manager;

public class GenericsMapper : Dictionary<ITypeDefinition, IStructBody?>
{
    public KeyValuePair<ITypeDefinition, IStructBody?> GetKeyValue(string name) => this.First(x => x.Key.Name == name);
    public IStructBody? GetValue(string name) => GetKeyValue(name).Value;
    public bool ContainsKey(string name) => this.FindFirstOrNullValue(x => x.Key.Name == name) is { };
}
