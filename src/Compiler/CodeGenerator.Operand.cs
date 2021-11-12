using Extensions;
using Roku.Declare;
using Roku.IntermediateCode;
using Roku.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Roku.Compiler;

public static partial class CodeGenerator
{
    public static void AssemblyOperandEmit(ILWriter il, IOperand op, INamespace ns, TypeMapper m, Dictionary<LabelCode, string> labels, GenericsMapper g)
    {
        il.WriteLine();
        if (op is IReturnBind prop && prop.Return is { } && m[prop.Return].Type == VariableType.Property)
        {
            il.WriteLine(LoadValue(m, m[prop.Return].Reciever!));
        }

        switch (op.Operator)
        {
            case Operator.Bind:
                if (op is IReturnBind r && m[r.Return!].Struct is NamespaceBody) return;
                var bind = op.Cast<Code>();
                il.WriteLine(LoadValue(m, bind.Left!));
                break;

            case Operator.Call:
                {
                    var call = op.Cast<Call>();
                    var f = m[call.Function.Function].Struct!.Cast<FunctionMapper>();
                    if (f.Function is FunctionTypeBody || f.Function is AnonymousFunctionBody)
                    {
                        il.WriteLine(LoadValue(m, call.Function.Function));
                    }
                    var args = call.Function.Arguments.Select((x, i) => LoadValue(m, x, GetArgumentType(ns, f, i))).ToArray();
                    var have_return = false;
                    if (f.Function is ExternFunction fx)
                    {
                        if (args.Length > 0) il.WriteLine(args.Join('\n'));
                        var callsig = (fx.Function.IsVirtual ? "callvirt" : "call") + (fx.Function.IsStatic ? "" : " instance");
                        var retvar = GetParameterName(fx.Function.ReturnType);
                        var asmname = $"[{fx.Assembly.GetName().Name}]";
                        var classname = GetTypeName(GetType(fx), Lookup.TypeMapperToGenericsMapper(f.TypeMapper));
                        var fname = fx.Function.Name;
                        var param_args = fx.Function.GetParameters().Select(x => GetParameterName(x.ParameterType)).Join(", ");
                        il.WriteLine($"{callsig} {retvar} class {asmname}{classname}::{fname}({param_args})");
                        have_return = fx.Function.ReturnType is { } p && p != typeof(void);
                    }
                    else if (f.Function is FunctionBody fb)
                    {
                        if (args.Length > 0) il.WriteLine(args.Join('\n'));
                        il.WriteLine($"call {GetTypeName(f.TypeMapper, fb.Return, g)} {EscapeILName(fb.Name)}({fb.Arguments.Select(a => GetTypeName(f.TypeMapper[a.Name], g)).Join(", ")})");
                        have_return = fb.Return is { };
                    }
                    else if (f.Function is FunctionTypeBody ftb)
                    {
                        if (args.Length > 0) il.WriteLine(args.Join('\n'));
                        il.WriteLine($"callvirt instance {(ftb.Return is null ? "void" : $"!{ftb.Arguments.Count}")} {GetFunctionTypeName(ftb)}::Invoke({Enumerable.Range(0, ftb.Arguments.Count).Select(x => $"!{x}").Join(", ")})");
                        have_return = ftb.Return is { };
                    }
                    else if (f.Function is AnonymousFunctionBody afb)
                    {
                        if (args.Length > 0) il.WriteLine(args.Join('\n'));
                        var args_type = call.Function.Arguments.Select((x, i) => GetArgumentType(ns, f, i)).ToArray();
                        var return_type = afb.Return is { } rx ? Lookup.GetStructType(ns, rx, f.TypeMapper) : null;
                        il.WriteLine($"callvirt instance {(afb.Return is null ? "void" : $"!{afb.Arguments.Count}")} {GetFunctionTypeName(args_type!, return_type)}::Invoke({Enumerable.Range(0, afb.Arguments.Count).Select(x => $"!{x}").Join(", ")})");
                        have_return = afb.Return is { };
                    }
                    else if (f.Function is EmbeddedFunction ef)
                    {
                        il.WriteLine(ef.OpCode(f, args));
                        have_return = ef.Return is { };
                    }

                    if (have_return && call.Return is ImplicitReturnValue &&
                        ns.Cast<AnonymousFunctionBody>().Return is null)
                    {
                        il.WriteLine("pop");
                    }
                }
                break;

            case Operator.If:
                var if_ = op.Cast<IfCode>();
                il.WriteLine(LoadValue(m, if_.Condition));
                il.WriteLine($"brfalse {labels[if_.Else]}");
                break;

            case Operator.Goto:
                var goto_ = op.Cast<GotoCode>();
                il.WriteLine($"br {labels[goto_.Label]}");
                break;

            case Operator.Label:
                var label = op.Cast<LabelCode>();
                il.Indent--;
                il.WriteLine($"{labels[label]}:");
                il.Indent++;
                break;

            case Operator.TypeBind:
                break;

            case Operator.IfCast:
                {
                    var ifcast = op.Cast<IfCastCode>();

                    var is_value = Lookup.IsValueType(m[ifcast.Condition].Struct);
                    var load_cond = LoadValue(m, ifcast.Condition);
                    il.WriteLine(load_cond);
                    if (is_value)
                    {
                        var value_type = m[ifcast.Condition].Struct!.Cast<ExternStruct>();

                        il.WriteLine($"box {GetILClassName(value_type)}");
                        il.WriteLine(StoreValue(m, m.CastBoxCondition));
                        il.WriteLine(load_cond = LoadValue(m, m.CastBoxCondition));
                    }

                    if (m[ifcast.Name].Struct is ExternStruct sx)
                    {
                        il.WriteLine($"isinst {GetILClassName(sx)}");

                        if (Lookup.IsValueType(m[ifcast.Name].Struct))
                        {
                            il.WriteLine($"brfalse.s {labels[ifcast.Else]}");
                            il.WriteLine(load_cond);
                            il.WriteLine($"unbox.any {GetILClassName(sx)}");
                            il.WriteLine(StoreValue(m, ifcast.Name));
                        }
                        else
                        {
                            il.WriteLine(StoreValue(m, ifcast.Name));
                            il.WriteLine(LoadValue(m, ifcast.Name));
                            il.WriteLine($"ldnull");
                            il.WriteLine($"cgt.un");
                            il.WriteLine($"brfalse.s {labels[ifcast.Else]}");
                        }
                    }
                    else if (m[ifcast.Name].Struct is StructBody sb)
                    {
                        il.WriteLine($"isinst {GetStructName(sb)}");
                        il.WriteLine($"ldnull");
                        il.WriteLine($"cgt.un");
                        il.WriteLine($"brfalse.s {labels[ifcast.Else]}");
                        il.WriteLine(load_cond);
                        il.WriteLine(StoreValue(m, ifcast.Name));
                    }
                    else if (m[ifcast.Name].Struct is NullBody)
                    {
                        il.WriteLine($"ldnull");
                        il.WriteLine($"ceq");
                        il.WriteLine($"brfalse.s {labels[ifcast.Else]}");
                        il.WriteLine($"ldnull");
                        il.WriteLine(StoreValue(m, ifcast.Name));
                    }
                }
                break;

            default:
                throw new Exception();
        }

        if (op is IReturnBind ret && ret.Return is { } && !(ret.Return is ImplicitReturnValue))
        {
            il.WriteLine(StoreValue(m, ret.Return));
        }
    }

    public static IStructBody? GetArgumentType(INamespace caller, FunctionMapper body, int index)
    {
        switch (body.Function)
        {
            case FunctionBody fb: return Lookup.GetStructType(caller, fb.Arguments[index].Type, body.TypeMapper);
            case AnonymousFunctionBody afb: return Lookup.GetStructType(caller, afb.Arguments[index].Type, body.TypeMapper);
            case EmbeddedFunction ef: return Lookup.GetStructType(caller, ef.Arguments[index], body.TypeMapper);
        }
        return null;
    }

    public static string LoadValue(TypeMapper m, IEvaluable value, IStructBody? target = null)
    {
        switch (value)
        {
            case StringValue x:
                return $"ldstr \"{x.Value}\"";

            case NumericValue x:
                {
                    var isbox = IsClassType(target);
                    if (m[x].Struct is ExternStruct es && es.Struct == typeof(long).GetTypeInfo())
                    {
                        return
                            (x.Value <= 8 ? $"ldc.i4.{x.Value}\nconv.i8"
                            : x.Value <= sbyte.MaxValue ? $"ldc.i4.s {x.Value}\nconv.i8"
                            : x.Value <= int.MaxValue ? $"ldc.i4 {x.Value}\nconv.i8"
                            : $"ldc.i8 {x.Value}") +
                            (isbox ? $"\nbox {GetStructName(m[x].Struct)}" : "");
                    }
                    else
                    {
                        return
                            (x.Value <= 8 ? $"ldc.i4.{x.Value}"
                            : x.Value <= sbyte.MaxValue ? $"ldc.i4.s {x.Value}"
                            : x.Value <= int.MaxValue ? $"ldc.i4 {x.Value}"
                            : $"ldc.i8 {x.Value}") +
                            (isbox ? $"\nbox {GetStructName(m[x].Struct)}" : "");
                    }
                }

            case BooleanValue x:
                return $"ldc.i4.{(x.Value ? 1 : 0)}";

            case NullValue:
                return "ldnull";

            case FloatingNumericValue x:
                {
                    var isbox = IsClassType(target);
                    var r = (m[x].Struct is ExternStruct es && es.Struct == typeof(double)) ? "r8" : "r4";

                    return
                        $"ldc.{r} {x.Value}" +
                        (isbox ? $"\nbox {GetStructName(m[x].Struct)}" : "");
                }

            case VariableValue _:
            case TemporaryValue _:
                {
                    var detail = m[value];
                    var isbox = Lookup.IsValueType(detail.Struct) && IsClassType(target);
                    if (detail.Type == VariableType.Argument)
                    {
                        return
                            (detail.Index <= 3 ? $"ldarg.{detail.Index}"
                            : detail.Index <= byte.MaxValue ? $"ldarg.s {detail.Index}"
                            : $"ldarg {detail.Index}") +
                            (isbox ? $"\nbox {GetStructName(detail.Struct)}" : "");
                    }
                    else
                    {
                        return
                            (detail.Index <= 3 ? $"ldloc.{detail.Index}"
                            : detail.Index <= byte.MaxValue ? $"ldloc.s {detail.Index}"
                            : $"ldloc {detail.Index}") +
                            (isbox ? $"\nbox {GetStructName(detail.Struct)}" : "");
                    }
                }

            case PropertyValue x:
                return $"{LoadValue(m, x.Left)}\nldfld {GetStructName(m[x].Struct)} {GetStructName(m[x.Left].Struct)}::{EscapeILName(x.Right)}";

            case ArrayContainer x:
                return $"newobj instance void {GetStructName(m[x].Struct)}::.ctor()\n{x.Values.Select(v => "dup\n" + LoadValue(m, v) + $"\ncallvirt instance void {GetStructName(m[x].Struct)}::Add(!0)").Join("\n")}";

            case FunctionReferenceValue x:
                return $"ldnull\nldftn {GetFunctionName(x, m[x].Struct!)}\nnewobj instance void {GetStructName(m[x].Struct)}::.ctor(object, native int)";

            case TypeValue x:
                return GetStructName(m[x].Struct);
        }
        throw new Exception();
    }

    public static string StoreValue(TypeMapper m, IEvaluable value)
    {
        switch (value)
        {
            case VariableValue _:
            case TemporaryValue _:
                var detail = m[value];
                if (detail.Type == VariableType.Argument)
                {
                    return
                        detail.Index <= 3 ? $"starg.{detail.Index}"
                        : detail.Index <= byte.MaxValue ? $"starg.s {detail.Index}"
                        : $"starg {detail.Index}";
                }
                else if (detail.Type == VariableType.LocalVariable)
                {
                    return
                        detail.Index <= 3 ? $"stloc.{detail.Index}"
                        : detail.Index <= byte.MaxValue ? $"stloc.s {detail.Index}"
                        : $"stloc {detail.Index}";
                }
                else if (detail.Type == VariableType.Property)
                {
                    return $"stfld {GetStructName(detail.Struct!)} {GetStructName(m[detail.Reciever!].Struct!)}::{EscapeILName(detail.Name)}";
                }
                break;

            case PropertyValue x:
                return $"stfld {GetStructName(m[x].Struct)} {GetStructName(m[x.Left].Struct)}::{EscapeILName(x.Right)}";
        }
        throw new Exception();
    }
}
