using Extensions;
using Roku.Declare;
using Roku.IntermediateCode;
using Roku.Manager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Compiler;

public static partial class CodeGenerator
{
    public static void AssemblyFunctionEmit(ILWriter il, List<FunctionSpecialization> fss)
    {
        for (var i = 0; i < fss.Count; i++)
        {
            var f = fss[i].Body.Cast<IFunctionBody>();
            var g = fss[i].GenericsMapper;
            var mapper = Lookup.GetTypemapper(f.SpecializationMapper, g);

            il.WriteLine($".method public static {GetTypeName(mapper, f.Return, g)} {EscapeILName(f.Name)}({f.Arguments.Select(a => GetTypeName(mapper[a.Name], g)).Join(", ")})");
            il.WriteLine("{");
            il.Indent++;
            if (i == 0) il.WriteLine(".entrypoint");
            il.WriteLine($".maxstack {f.Body.Select(x => GetMaxstack(x, mapper)).Max()}");
            var local_vals = GetLocalValues(mapper);
            if (local_vals.Count > 0)
            {
                il.WriteLine(".locals(");
                il.Indent++;
                il.WriteLine(local_vals.Select(x => $"[{x.Index}] {GetTypeName(x, g)} {x.Name}").Join(",\n"));
                il.Indent--;
                il.WriteLine(")");
            }
            var labels = Lookup.AllLabels(f.Body).Zip(Lists.Sequence(1)).ToDictionary(x => x.First, x => $"_{x.First.Name}{x.Second}");
            f.Body.Each(x => AssemblyOperandEmit(il, x, f, mapper, labels, g));
            il.WriteLine("ret");
            il.Indent--;
            il.WriteLine("}");
        }
    }

    public static List<VariableDetail> GetLocalValues(TypeMapper mapper)
    {
        var local_vals = mapper.Values
            .Where(x => (x.Type == VariableType.LocalVariable && !(x.Struct is NamespaceBody || (x.Struct is AnonymousFunctionBody afb && afb.Generics.Count > 0))) ||
                (x.Type == VariableType.FunctionMapper && x.Struct!.Cast<FunctionMapper>().Function is AnonymousFunctionBody afb2 && afb2.Generics.Count > 0))
            .Sort((a, b) => a.Index - b.Index)
            .ToList();
        local_vals.Each((x, i) => x.Index = i);
        return local_vals;
    }

    public static int GetMaxstack(IOperand op, TypeMapper m)
    {
        var stack_size = 0;
        switch (op.Operator)
        {
            case Operator.Call:
                {
                    var call = op.Cast<Call>();
                    var fm = m[call.Function.Function].Struct!.Cast<FunctionMapper>();
                    stack_size = call.Function.Arguments.Select(x => GetMaxstack(x)).Sum();
                    if (fm.Function is FunctionTypeBody || fm.Function is AnonymousFunctionBody) stack_size++;
                    break;
                }

            case Operator.Bind:
                {
                    var bind = op.Cast<Code>();
                    stack_size = GetMaxstack(bind.Left!);
                    break;
                }

            case Operator.If:
            case Operator.IfCast:
                stack_size = 2;
                break;
        }
        stack_size = Math.Max(stack_size, op is IReturnBind prop && prop.Return is { } ? 1 : 0);
        return stack_size + (op is IReturnBind prop2 && prop2.Return is { } && m[prop2.Return].Type == VariableType.Property ? 1 : 0);
    }

    public static int GetMaxstack(IEvaluable e) =>
        e is ArrayContainer ? 3
        : e is FunctionReferenceValue ? 2
        : 1;
}
