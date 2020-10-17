﻿namespace Roku.Manager
{
    public class TypeSpecialization : IStructBody, INamespace
    {
        public string Name { get => Body.Name; }
        public IStructBody Body { get; }
        public GenericsMapper GenericsMapper { get; }


        public TypeSpecialization(IStructBody body, GenericsMapper gen_map)
        {
            Body = body;
            GenericsMapper = gen_map;
        }
    }
}
