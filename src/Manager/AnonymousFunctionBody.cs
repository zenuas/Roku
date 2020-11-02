using Extensions;
using Roku.Declare;
using Roku.IntermediateCode;
using System.Collections.Generic;

namespace Roku.Manager
{
    public class AnonymousFunctionBody : IFunctionBody, ILexicalScope, INamespace
    {
        public string Name { get; }
        public ITypeDefinition? Return { get; set; } = null;
        public List<(VariableValue Name, ITypeDefinition? Type)> Arguments { get; } = new List<(VariableValue, ITypeDefinition?)>();
        public bool IsImplicit { get; set; } = true;
        public List<IOperand> Body { get; } = new List<IOperand>();
        public INamespace Namespace { get; }
        public ILexicalScope? Parent { get; } = null;
        public Dictionary<string, IEvaluable> LexicalScope { get; } = new Dictionary<string, IEvaluable>();
        public int MaxTemporaryValue { get; set; } = 0;

        public AnonymousFunctionBody(RootNamespace root, string system_unique_name)
        {
            Namespace = root;
            Name = system_unique_name;
        }

        public override string ToString() => $"{{{Arguments.Map(x => x.Name.ToString() + (x.Type is { } t ? $" : {t}" : "")).Join(", ")}{(Return is { } r ? $" => {r}" : "")}}}";
    }
}
