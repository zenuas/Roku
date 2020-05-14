using System.Collections.Generic;

namespace Roku.Manager
{
    public interface ISpecialization
    {
        public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; }
    }
}
