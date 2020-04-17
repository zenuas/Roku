using Extensions;
using Roku.Manager;
using Roku.TypeSystem;

namespace Roku.Compiler
{
    public static class CodeGenerator
    {
        public static void Emit(SourceCodeBody body, string path)
        {
            var pgms = Lookup.AllPrograms(body);
            var nss = Lookup.AllNamespaces(body);
            var externs = Lookup.AllExternFunctions(nss);
            var extern_asms = externs.Map(GetAssembly).ToArray();
        }

        public static System.Reflection.Assembly GetAssembly(ExternFunction e)
        {
            return ((RkCILFunction)e.Function).MethodInfo.GetType().Assembly;
        }
    }
}
