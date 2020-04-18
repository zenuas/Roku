using Extensions;
using Roku.TypeSystem;
using System;
using System.Reflection;

namespace Roku.Manager
{
    public class RootNamespace : NamespaceManager
    {
        public RootNamespace() : base("")
        {
        }

        public static RkCILStruct LoadType(RootNamespace root, Type t)
        {
            return LoadType(root, t.Name, t);
        }

        public static RkCILStruct LoadType(RootNamespace root, string name, Type t)
        {
            return LoadType(root, name, t.GetTypeInfo());
        }

        public static RkCILStruct LoadType(RootNamespace root, TypeInfo ti)
        {
            return LoadType(root, ti.Name, ti);
        }

        public static RkCILStruct LoadType(RootNamespace root, string name, TypeInfo ti)
        {
            var st = root.Structs.Where(x => x.Struct is RkCILStruct && x.Struct.Name == name).FirstOrNull();
            if (st is { }) return (RkCILStruct)st.Struct;

            var t = new RkCILStruct(name, ti);
            root.Structs.Add(new StructBody(t));
            return t;
        }

        public static RkCILStruct? LoadTypeWithoutVoid(RootNamespace root, Type t)
        {
            return LoadTypeWithoutVoid(root, t.GetTypeInfo());
        }

        public static RkCILStruct? LoadTypeWithoutVoid(RootNamespace root, TypeInfo ti)
        {
            return ti == typeof(void) ? null : LoadType(root, ti.Name, ti);
        }

        public static RkCILFunction LoadFunction(RootNamespace root, MethodInfo method)
        {
            return LoadFunction(root, method.Name, method);
        }

        public static RkCILFunction LoadFunction(RootNamespace root, string name, MethodInfo method)
        {
            var f = new RkCILFunction(name, method);
            f.Return = LoadTypeWithoutVoid(root, method.ReturnType);
            method.GetParameters().Each(x => f.Arguments.Add(LoadType(root, x.ParameterType)));
            return f;
        }
    }
}
