using Extensions;
using Roku.IntermediateCode;
using Roku.Manager;
using Roku.TypeSystem;
using System.Collections.Generic;

namespace Roku.Compiler
{
    public static class Lookup
    {
        public static List<SourceCodeBody> AllPrograms(SourceCodeBody src)
        {
            var readed = new HashSet<SourceCodeBody>();

            List<SourceCodeBody> use_load(SourceCodeBody source)
            {
                var xs = new List<SourceCodeBody>();
                if (!readed.Contains(source))
                {
                    xs.Add(source);
                    readed.Add(source);
                }
                xs.AddRange(source.Uses.By<SourceCodeBody>().Map(xs => use_load(xs)).Flatten());
                return xs;
            }
            return use_load(src);
        }

        public static IEnumerable<(FunctionBody Body, SourceCodeBody Source)> AllFunctionBodies(List<SourceCodeBody> srcs)
        {
            return srcs.Map(x => x.Functions.By<FunctionBody>().Zip(Lists.Repeat(x))).Flatten();
        }

        public static IFunction? FindFunction(SourceCodeBody src, string name, List<ITypedValue> args)
        {
            var f = src.Functions.FindFirstOrNull(x => x.Function.Name == name && FunctionArgumentsEquals(x.Function.Arguments, args));
            if (f is { }) return f.Function;

            return src.Uses.Map(x => FindFunction(x, name, args)).By<IFunction>().FirstOrNull();
        }

        public static IFunction? FindFunction(INamespace ns, string name, List<ITypedValue> args)
        {
            return ns.Functions.FindFirstOrNull(x => x.Function.Name == name && FunctionArgumentsEquals(x.Function.Arguments, args))?.Function;
        }

        public static bool FunctionArgumentsEquals(List<IType> source, List<ITypedValue> args)
        {
            if (source.Count != args.Count) return false;
            return source.Zip(args).And(x => TypeEquals(x.First, x.Second));
        }

        public static bool TypeEquals(IType source, ITypedValue arg)
        {
            return true;
        }

        public static IEnumerable<ITypedValue> AllValues(Operand op)
        {
            switch (op)
            {
                case Call x:
                    foreach (var arg in x.Arguments) yield return arg;
                    if (x.FirstLookup is { }) yield return x.FirstLookup;
                    break;
            }
        }
    }
}
