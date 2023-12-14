using Extensions;
using Roku.Declare;
using Roku.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Roku.Compiler;

public static partial class CodeGenerator
{
    public static Type GetType(ExternFunction e) => e.DeclaringType ?? e.Function.DeclaringType!;

    public static string GetTypeName(TypeMapper m, IEvaluable? e, GenericsMapper g) => e is null ? "void" : GetStructName(GetType(m[e], g));

    public static string GetTypeName(VariableDetail vd, GenericsMapper g) => GetStructName(GetType(vd, g));

    public static string GetTypeName(Type t, GenericsMapper g) => t.FullName! + (!t.IsGenericType ? "" : $"<{t.GetGenericArguments().Select(x => GetStructName(g.GetValue(x.Name))).Join(", ")}>");

    public static string GetParameterName(Type t) =>
        t.IsGenericTypeParameter ? $"!{t.GenericParameterPosition}"
        : t.IsGenericMethodParameter ? $"!!{t.GenericParameterPosition}"
        : GetILStructName(t);

    public static IStructBody? GetType(VariableDetail vd, GenericsMapper g) => GetType(vd.Struct, g);

    public static IStructBody? GetType(IStructBody? body, GenericsMapper g) => body is GenericsParameter gp ? g.First(x => x.Key.Name == gp.Name).Value : body;

    public static string GetStructName(IStructBody? body, bool escape = true)
    {
        switch (body)
        {
            case null: return "void";
            case ExternStruct x: return GetILStructName(x);
            case NumericStruct x: return GetStructName(x.Types.First());
            case StructBody x: return $"class {(escape ? EscapeILName(x.Name) : x.Name)}";
            case StructSpecialization x when x.Body is ExternStruct e: return $"class [{e.Assembly.GetName().Name}]{e.Struct.FullName}{GetGenericsName(e, x.GenericsMapper)}";
            case StructSpecialization x when x.Body is StructBody e: return $"class {GetStructName(x.Name, e, x.GenericsMapper, false).To(x => escape ? EscapeILName(x) : x)}";
            case EnumStructBody _: return "object";
            case NullBody _: return "object";
            case AnonymousFunctionBody x: return GetFunctionTypeName(x);
            case FunctionTypeBody x: return GetFunctionTypeName(x);
            case FunctionMapper x when x.Function is FunctionTypeBody ftb: return GetFunctionTypeName(ftb);
            case FunctionMapper x when x.Function is AnonymousFunctionBody afb: return GetFunctionTypeName(afb, x.TypeMapper);
        }
        throw new();
    }

    public static string GetStructName(string name, ISpecialization sp, GenericsMapper g, bool escape = true) => (g.Count == 0 ? name : $"{name}{GetGenericsName(sp, g, false)}").To(x => escape ? EscapeILName(x) : x);

    public static string GetFunctionTypeName(AnonymousFunctionBody anon) => GetFunctionTypeName(anon, anon.SpecializationMapper.First().Value);

    public static string GetFunctionTypeName(AnonymousFunctionBody anon, TypeMapper mapper)
    {
        var g = Lookup.TypeMapperToGenericsMapper(mapper);
        var args_type = anon.Arguments.Select(x => GetType(mapper[x.Type], g)).ToArray();
        var return_type = anon.Return is { } rx ? GetType(mapper[rx], g) : null;
        return GetFunctionTypeName(args_type!, return_type);
    }

    public static string GetFunctionTypeName(FunctionTypeBody t) => GetFunctionTypeName([.. t.Arguments], t.Return);

    public static string GetFunctionTypeName(IStructBody[] args, IStructBody? ret)
    {
        if (ret is { } r)
        {
            return $"class [mscorlib]System.Func`{args.Length + 1}<{args.Concat(r).Select(x => GetStructName(x)).Join(", ")}>";
        }
        else
        {
            if (args.Length == 0) return "class [mscorlib]System.Action";
            return $"class [mscorlib]System.Action`{args.Length}<{args.Select(x => GetStructName(x)).Join(", ")}>";
        }
    }

    public static string GetFunctionName(IStructBody body)
    {
        if (body is AnonymousFunctionBody anon)
        {
            return GetFunctionName(anon, anon.SpecializationMapper.Values.First());
        }
        throw new();
    }

    public static string GetFunctionName(AnonymousFunctionBody anon, TypeMapper mapper)
    {
        var g = Lookup.TypeMapperToGenericsMapper(mapper);
        return $"{GetTypeName(mapper, anon.Return, g)} {EscapeILName(anon.Name)}({anon.Arguments.Select(x => GetTypeName(mapper, x.Type, g)).Join(", ")})";
    }

    public static string EscapeILName(string s) => !CilReservedWord.Contains(s) && Regex.IsMatch(s, @"^[_#$@a-zA-Z][_?$@`a-zA-Z0-9]*$") ? s : $"'{s}'";

    public static string GetGenericsName(ISpecialization sp, GenericsMapper g, bool inner_escape = true) => $"<{sp.Generics.Select(x => GetStructName(g[x], inner_escape)).Join(", ")}>";

    public static string GetILStructName(ExternStruct sx) => GetILStructName(sx.Struct, sx.Assembly);

    public static string GetILStructName(Type t, Assembly? asm = null)
    {
        if (t == typeof(void)) return "void";
        if (t == typeof(string)) return "string";
        if (t == typeof(int)) return "int32";
        if (t == typeof(long)) return "int64";
        if (t == typeof(short)) return "int16";
        if (t == typeof(byte)) return "byte";
        if (t == typeof(double)) return "float64";
        if (t == typeof(float)) return "float32";
        if (t == typeof(bool)) return "bool";
        if (t == typeof(object)) return "object";
        return GetILClassName(t, asm ?? t.Assembly);
    }

    public static bool IsClassType(IStructBody? body) => (body is { } && !Lookup.IsValueType(body));

    public static string GetILClassName(ExternStruct sx) => GetILClassName(sx.Struct, sx.Assembly);

    public static string GetILClassName(Type t, Assembly asm)
    {
        var gen = "";
        var gens = t.GetGenericArguments();
        if (gens.Length > 0)
        {
            gen = $"<{gens.Select(x => GetILStructName(x)).Join(", ")}>";
        }
        return $"{(t.IsValueType ? "" : "class ")}[{asm.GetName().Name}]{t.Namespace}.{t.Name}{gen}";
    }

    public static HashSet<string> CilReservedWord = [
        "abstract",
        "add",
        "addon",
        "algorithm",
        "alignment",
        "and",
        "ansi",
        "any",
        "arglist",
        "array",
        "as",
        "assembly",
        "assert",
        "at",
        "auto",
        "autochar",
        "beforefieldinit",
        "beq",
        "bestfit",
        "bge",
        "bgt",
        "ble",
        "blob",
        "blob_object",
        "blt",
        "bne",
        "bool",
        "box",
        "br",
        "break",
        "brfalse",
        "brtrue",
        "bstr",
        "bytearray",
        "byvalstr",
        "call",
        "calli",
        "callmostderived",
        "callvirt",
        "carray",
        "castclass",
        "catch",
        "cctor",
        "cdecl",
        "ceq",
        "cf",
        "cgt",
        "char",
        "charmaperror",
        "cil",
        "ckfinite",
        "class",
        "clsid",
        "clt",
        "conv",
        "corflags",
        "cpblk",
        "cpobj",
        "ctor",
        "currency",
        "custom",
        "data",
        "date",
        "decimal",
        "default",
        "demand",
        "deny",
        "div",
        "dup",
        "emitbyte",
        "endfilter",
        "endfinally",
        "entrypoint",
        "enum",
        "error",
        "event",
        "explicit",
        "export",
        "extends",
        "extern",
        "false",
        "famandassem",
        "family",
        "famorassem",
        "fastcall",
        "fault",
        "field",
        "file",
        "filetime",
        "filter",
        "final",
        "finally",
        "fire",
        "fixed",
        "float",
        "float32",
        "float64",
        "forwardref",
        "fromunmanaged",
        "get",
        "handler",
        "hash",
        "hidebysig",
        "hresult",
        "idispatch",
        "imagebas",
        "imagebase",
        "implements",
        "import",
        "in",
        "inheritcheck",
        "initblk",
        "initobj",
        "initonly",
        "instance",
        "int",
        "int16",
        "int32",
        "int64",
        "int8",
        "interface",
        "internalcall",
        "isinst",
        "iunknown",
        "ixed",
        "jmp",
        "language",
        "lasterr",
        "ldarg",
        "ldarga",
        "ldc",
        "ldelem",
        "ldelema",
        "ldfld",
        "ldflda",
        "ldftn",
        "ldlen",
        "ldloc",
        "ldloca",
        "ldnull",
        "ldobj",
        "ldsfld",
        "ldsflda",
        "ldstr",
        "ldtoken",
        "ldvirtftn",
        "leave",
        "line",
        "linkcheck",
        "literal",
        "locale",
        "localloc",
        "locals",
        "lpstr",
        "lpstruct",
        "lpwstr",
        "managed",
        "marshal",
        "maxstack",
        "method",
        "mkrefany",
        "modopt",
        "modreq",
        "module",
        "mresource",
        "mul",
        "namespace",
        "native",
        "neg",
        "nested",
        "newarr",
        "newobj",
        "newslot",
        "noappdomain",
        "noinlining",
        "nomachine",
        "nomangle",
        "nometadata",
        "noncasdemand",
        "noncasinheritance",
        "noncaslinkdemand",
        "nop",
        "noprocess",
        "not",
        "notserialized",
        "null",
        "nullref",
        "object",
        "objectref",
        "off",
        "on",
        "optil",
        "or",
        "other",
        "out",
        "override",
        "pack",
        "param",
        "permission",
        "permissionset",
        "permitonly",
        "pinned",
        "pinvokeimpl",
        "pop",
        "prejitdeny",
        "prejitgrant",
        "preservesig",
        "private",
        "privatescope",
        "property",
        "public",
        "publickey",
        "publickeytoken",
        "record",
        "ref",
        "refanytype",
        "refanyval",
        "rem",
        "removeon",
        "reqmin",
        "reqopt",
        "reqrefuse",
        "reqsecobj",
        "request",
        "ret",
        "retainappdomain",
        "retargetable",
        "rethrow",
        "rtspecialname",
        "runtime",
        "safearray",
        "sealed",
        "sequential",
        "serializable",
        "set",
        "shl",
        "shr",
        "size",
        "sizeof",
        "specialname",
        "starg",
        "static",
        "stdcall",
        "stelem",
        "stfld",
        "stind",
        "stloc",
        "stobj",
        "storage",
        "stored_object",
        "stream",
        "streamed_object",
        "strict",
        "string",
        "struct",
        "stsfld",
        "sub",
        "subsystem",
        "switch",
        "synchronized",
        "syschar",
        "sysstring",
        "tail",
        "tbstr",
        "thiscall",
        "throw",
        "tls",
        "to",
        "true",
        "try",
        "typedref",
        "unaligned",
        "unbox",
        "unicode",
        "unmanaged",
        "unmanagedexp",
        "unsigned",
        "userdefined",
        "value",
        "valuetype",
        "vararg",
        "variant",
        "vector",
        "ver",
        "virtual",
        "void",
        "volatile",
        "vtable",
        "vtentry",
        "vtfixup",
        "winapi",
        "with",
        "xor",
        "zeroinit",
    ];
}
