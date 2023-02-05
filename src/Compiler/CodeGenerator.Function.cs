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
    public static string AppendFunctionSpecialization(List<(string Name, FunctionSpecialization Function)> fss, FunctionSpecialization f)
    {
        var find = FindFunctionSpecialization(fss, f.Body, f.GenericsMapper);
        if (find.Exists) return find.Name;
        if (find.Name == "")
        {
            var name = f.Body.Name;
            var i = 0;
            while (true)
            {
                if (!fss.Exists(x => x.Name == name)) break;
                name = $"{f.Body.Name}#{++i}";
            }
            fss.Add((name, f));
            return name;
        }
        else
        {
            fss.Add((find.Name, f));
            return find.Name;
        }
    }

    public static (bool Exists, string Name) FindFunctionSpecialization(List<(string Name, FunctionSpecialization Function)> fss, IFunctionName body, GenericsMapper g)
    {
        var appended = fss.FindFirstOrNullValue(x => x.Function.Body == body && EqualsGenericsMapper(x.Function.GenericsMapper, g));
        if (appended.HasValue) return (true, appended.Value.Name);

        var sameinst = fss.FindFirstOrNullValue(x => x.Function.Body == body);
        if (sameinst.HasValue) return (false, sameinst.Value.Name);

        return (false, "");
    }

    public static bool EqualsGenericsMapper(GenericsMapper g1, GenericsMapper g2) => g1.SequenceEqual(g2);

    public static void AssemblyFunctionEmit(ILWriter il, List<(string Name, FunctionSpecialization Function)> fss)
    {
        for (var i = 0; i < fss.Count; i++)
        {
            var f = fss[i].Function.Body.Cast<IFunctionBody>();
            var g = fss[i].Function.GenericsMapper;
            var mapper = Lookup.GetTypemapper(f.SpecializationMapper, g);

            il.WriteLine($".method public static {GetTypeName(mapper, f.Return, g)} {EscapeILName(fss[i].Name)}({f.Arguments.Select(a => GetTypeName(mapper[a.Name], g)).Join(", ")})");
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
            f.Body.Each(x => AssemblyOperandEmit(il, x, f, mapper, labels, g, fss));
            il.WriteLine("ret");
            il.Indent--;
            il.WriteLine("}");
        }
    }

    public static List<VariableDetail> GetLocalValues(TypeMapper mapper)
    {
        var local_vals = mapper.Values
            .Where(x => (x.Type == VariableType.LocalVariable && x.Struct is not NamespaceBody) ||
                (x.Type == VariableType.FunctionMapper && IsCallableAnonymousFunction(x)))
            .Order((a, b) => a.Index - b.Index)
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
                    var bind = op.Cast<BindCode>();
                    stack_size = GetMaxstack(bind.Value!);
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
