using Roku.IntermediateCode;
using Roku.Manager;
using System.Collections.Generic;

namespace Roku.Compiler
{
    public class FunctionCaller
    {
        public IFunctionBody Body { get; }
        public Dictionary<TypeValue, IStructBody?> GenericsMapper { get; }

        public FunctionCaller(IFunctionBody body, Dictionary<TypeValue, IStructBody?> gen_map)
        {
            Body = body;
            GenericsMapper = gen_map;
        }
    }
}
