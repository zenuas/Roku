using Roku.Declare;
using System.Collections.Generic;

namespace Roku.Manager
{
    public interface ISpecialization
    {
        public List<TypeGenericsParameter> Generics { get; }
        public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; }
    }
}
