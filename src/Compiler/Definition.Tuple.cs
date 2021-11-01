﻿using Extensions;
using Roku.Declare;
using Roku.IntermediateCode;
using Roku.Manager;

namespace Roku.Compiler
{
    public static partial class Definition
    {
        public static FunctionBody TupleFunctionDefinition(RootNamespace root, int count)
        {
            var name = GetTupleName(count);
            var fbody = MakeFunction(root, name);
            var fret = new TypeSpecialization(new VariableValue(name));
            var fcall = new TypeSpecialization(new VariableValue(name));
            var self = new VariableValue("$self");
            fbody.LexicalScope.Add(self.Name, self);
            fbody.Body.Add(new Call(new FunctionCallValue(fcall)) { Return = self });

            Lists.Range(0, count).Each(i =>
            {
                var gp = new TypeGenericsParameter($"t{i + 1}");
                var farg_var = new VariableValue($"x{i + 1}");
                fbody.Generics.Add(gp);
                fbody.Arguments.Add((farg_var, gp));
                fbody.LexicalScope.Add(farg_var.Name, farg_var);
                fbody.Body.Add(new Code { Operator = Operator.Bind, Return = new PropertyValue(self, $"{i + 1}"), Left = farg_var });
                fret.Generics.Add(gp);
                fcall.Generics.Add(gp);
            });
            fbody.Body.Add(new Call(new FunctionCallValue(new VariableValue("return")).Return(x => x.Arguments.Add(self))));
            fbody.Return = fret;
            return fbody;
        }

        public static FunctionBody TupleBodyDefinition(RootNamespace root, int count)
        {
            var name = GetTupleName(count);
            var exists = root.Functions.FindFirstOrNull(x => x.Name == name);

            if (exists is FunctionBody fb) return fb;

            _ = TupleStructDefinition(root, count);
            return TupleFunctionDefinition(root, count);
        }

        public static StructBody TupleStructDefinition(RootNamespace root, int count)
        {
            var name = GetTupleName(count);
            var body = new StructBody(root, name);
            Lists.RangeTo(1, count).Each(i =>
            {
                var gp = new TypeGenericsParameter($"a{i}");
                body.Generics.Add(gp);
                var member = new VariableValue($"{i}");
                body.LexicalScope.Add(member.Name, member);
                body.Body.Add(new TypeBind(member, gp));
                body.Members.Add(member.Name, member);
            });
            root.Structs.Add(body);
            return body;
        }
    }
}
