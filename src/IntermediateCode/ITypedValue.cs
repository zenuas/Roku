using Roku.TypeSystem;

namespace Roku.IntermediateCode
{
    public interface ITypedValue
    {
        public IType? Type { get; set; }
    }
}
