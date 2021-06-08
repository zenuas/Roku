using Extensions;
using Roku.Declare;
using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class AnonymousFunctionBody : IFunctionBody, IStructBody, ILexicalScope
    {
        public string Name { get; }
        public ITypeDefinition? Return { get; set; } = null;
        public List<(VariableValue Name, ITypeDefinition Type)> Arguments { get; } = new List<(VariableValue, ITypeDefinition)>();
        public bool IsImplicit { get; set; } = true;
        public List<IOperand> Body { get; } = new List<IOperand>();
        public INamespace Namespace { get; }
        public ILexicalScope? Parent { get; set; } = null;
        public Dictionary<string, IEvaluable> LexicalScope { get; } = new Dictionary<string, IEvaluable>();
        public int MaxTemporaryValue { get; set; } = 0;
        public List<TypeGenericsParameter> Generics { get; } = new List<TypeGenericsParameter>();
        public Dictionary<GenericsMapper, TypeMapper> SpecializationMapper { get; } = new Dictionary<GenericsMapper, TypeMapper>();

        public AnonymousFunctionBody(INamespace ns, string system_unique_name)
        {
            Namespace = ns;
            Name = system_unique_name;
        }

        public override string ToString()
        {
            var m = SpecializationMapper.Values.FirstOrNull();
            static string type_name(TypeMapper? m, ITypeDefinition d)
            {
                if (m is { } && d is TypeImplicit && m.ContainsKey(d) && m[d].Struct is { } s)
                {
                    return s.ToString()!;
                }
                return d.ToString()!;
            }

            return $"{{{Arguments.Map(x => x.Name.ToString() + (x.Type is { } t ? $" : {type_name(m, t)}" : "")).Join(", ")}{(Return is { } r ? $" => {type_name(m, r)}" : "")}}}";
        }
    }
}
