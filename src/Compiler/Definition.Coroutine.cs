using Extensions;
using Roku.Declare;
using Roku.IntermediateCode;
using Roku.Manager;
using Roku.Node;
using System.Collections.Generic;
using System.Linq;

namespace Roku.Compiler;

public static partial class Definition
{
    public static void ConvertCoroutine(SourceCodeBody src, IScopeNode scope, FunctionBody body)
    {
        /*
            struct Co$0
                var state = 0
                var value: a
                var next: [Co$0 | Null]
                var local: CoLocal$0 # local value exist
        */
        var list_a = body.Constraints.FindFirst(x => x.Class.Name == "List" && x.Generics.Count == 2 && x.Generics[0] == body.Return).Generics[1];
        var co_struct = new StructBody(src, $"Co${src.CoroutineUniqueCount}");
        var co_struct_typename = new TypeValue() { Name = co_struct.Name };
        var state = new VariableValue() { Name = "state" };
        var value = new VariableValue() { Name = "value" };
        var next = new VariableValue() { Name = "next" };
        var local = new VariableValue() { Name = "local" };
        co_struct.Members.Add("state", co_struct.LexicalScope["state"] = state);
        co_struct.Members.Add("value", co_struct.LexicalScope["value"] = value);
        co_struct.Members.Add("next", co_struct.LexicalScope["next"] = next);
        co_struct.Body.Add(new Code { Operator = Operator.Bind, Return = state, Left = new NumericValue() { Value = 0 } });
        co_struct.Body.Add(new TypeBind(value, list_a));
        co_struct.Body.Add(new TypeBind(next, new TypeEnum(new ITypeDefinition[] { co_struct_typename, new TypeValue() { Name = "Null" } })));
        co_struct.SpecializationMapper[new GenericsMapper()] = new TypeMapper();
        src.Structs.Add(co_struct);

        /*
            sub Co$0() Co$0
                return(newobj Co$0.ctor())
        */
        src.Functions.Add(new EmbeddedFunction(co_struct.Name, co_struct.Name) { OpCode = (_, args) => $"newobj instance void {co_struct.Name}::.ctor()" });

        var local_value_exist = !body.LexicalScope.Where(x => x.Value is VariableValue).IsEmpty();
        var co_local = new StructBody(src, $"CoLocal${src.CoroutineUniqueCount++}") { IsCoroutineLocal = true };
        var co_local_typename = new TypeValue() { Name = co_local.Name };
        if (local_value_exist)
        {
            co_struct.Members.Add("local", co_struct.LexicalScope["local"] = local);
            co_struct.Body.Add(new TypeBind(local, co_local_typename));

            /*
                struct CoLocal$0
                    var args...
                    var local_values...
            */
            body.LexicalScope.Where(x => x.Value is VariableValue).Each(x => co_local.Members.Add(x.Key, co_local.LexicalScope[x.Key] = x.Value));
            body.Arguments.Each(x => co_local.Body.Add(new TypeBind(x.Name, x.Type)));
            co_local.SpecializationMapper[new GenericsMapper()] = new TypeMapper();
            src.Structs.Add(co_local);

            /*
                sub CoLocal$0() CoLocal$0
                    return(newobj CoLocal$0.ctor())
            */
            src.Functions.Add(new EmbeddedFunction(co_local.Name, co_local.Name) { OpCode = (_, args) => $"newobj instance void {co_local.Name}::.ctor()" });
        }

        /*
            sub next($self: Co$0) [a, Co$0]
                var $next_or_null = $self.next
                if var $next: Co$0 = $next_or_null
                    var $value = $self.value
                    var $ret = Tuple#2($value, $next)
                    return($ret)
                var $state = $self.state
                var $local = $self.local # local value exist
                var $cond =  $state == N
                if $cond then goto stateN_
                var $m1 = - 1
                $cond = $state == $m1
                if $cond then goto end_

                yield(x) ->
                    $self.value = x
                    $next = Co$0()
                    $next.state = N
                    $next.local = $local # local value exist
                    $self.next = $next
                    $ret = Tuple#2(x, $next)
                    return($ret)
                    stateN_:

                end_:
                $m1 = - 1
                $self.state = $m1
                $ret = Tuple#2(null, $self)
                return($ret)
        */
        var next_body = new FunctionBody(src, "next");
        var _self = new VariableValue() { Name = "$self" };
        next_body.Arguments.Add((_self, co_struct_typename));
        next_body.LexicalScope["$self"] = _self;
        var tuple2sp = new TypeSpecialization(new VariableValue() { Name = GetTupleName(2) });
        tuple2sp.Generics.Add(list_a);
        tuple2sp.Generics.Add(co_struct_typename);
        next_body.Return = tuple2sp;
        src.Functions.Add(next_body);

        var yield_count = body.Body.Where(IsYield).Count();
        var labels_jump = Lists.Sequence(1).Take(yield_count + 1).Select(n => new LabelCode() { Name = n > yield_count ? "end_" : $"state{n}_" }).ToList();
        var labels_cond = Lists.Sequence(0).Take(yield_count + 2).Select(n => new LabelCode() { Name = $"cond{n}_" }).ToList();

        var tuple2_body = TupleBodyDefinition(Lookup.GetRootNamespace(src), 2);
        var tuple2 = new VariableValue() { Name = tuple2_body.Name };
        var _next_or_null = new VariableValue() { Name = "$next_or_null" };
        var _next = new VariableValue() { Name = "$next" };
        var co_struct_name = new VariableValue() { Name = co_struct.Name };
        var _cond = new VariableValue() { Name = "$cond" };
        var _value = new VariableValue() { Name = "$value" };
        var _ret = new VariableValue() { Name = "$ret" };
        var _state = new VariableValue() { Name = "$state" };
        var _m1 = new VariableValue() { Name = "$m1" };
        var _local = new VariableValue() { Name = "$local" };
        next_body.LexicalScope["$next_or_null"] = _next_or_null;
        next_body.LexicalScope["$next"] = _next;
        next_body.LexicalScope["$value"] = _value;
        next_body.LexicalScope["$ret"] = _ret;
        next_body.LexicalScope["$state"] = _state;
        next_body.LexicalScope["$m1"] = _m1;
        body.LexicalScope.Where(x => x.Value is VariableValue).Each(x => next_body.LexicalScope[x.Key] = x.Value);

        next_body.Body.AddRange(new List<IOperand> {
                new Code { Operator = Operator.Bind, Return = _next_or_null, Left = new PropertyValue(_self, "next") },
                new IfCastCode(_next, co_struct_typename, _next_or_null, labels_cond[0]),
                new Code { Operator = Operator.Bind, Return = _value, Left = new PropertyValue(_self, "value") },
                new Call(new FunctionCallValue(tuple2).Return(x => x.Arguments.AddRange(new IEvaluable[] { _value, _next }))) { Return = _ret },
                new Call(new FunctionCallValue(new VariableValue() { Name = "return" }).Return(x => x.Arguments.Add(_ret))),
                labels_cond[0],
                new Code { Operator = Operator.Bind, Return = _state, Left = new PropertyValue(_self, "state") },
            });
        if (local_value_exist)
        {
            next_body.LexicalScope["$local"] = _local;
            next_body.Body.Add(new Code { Operator = Operator.Bind, Return = _local, Left = new PropertyValue(_self, "local") });
        }
        next_body.Body.AddRange(Lists.Sequence(1).Take(yield_count).Select(n => new IOperand[] {
                new Call(new FunctionCallValue(new VariableValue() { Name = "==" }).Return(x => x.Arguments.AddRange(new IEvaluable[] { _state, new NumericValue() { Value = (uint)n } }))) { Return = _cond },
                new IfCode(_cond, labels_cond[n]),
                new GotoCode(labels_jump[n - 1]),
                labels_cond[n],
            }).Flatten());
        next_body.Body.AddRange(new IOperand[] {
                new Call(new FunctionCallValue(new VariableValue() { Name = "-" }).Return(x => x.Arguments.Add(new NumericValue() { Value = 1 }))) { Return = _m1 },
                new Call(new FunctionCallValue(new VariableValue() { Name = "==" }).Return(x => x.Arguments.AddRange(new IEvaluable[] { _state, _m1 }))) { Return = _cond },
                new IfCode(_cond, labels_cond[^1]),
                new GotoCode(labels_jump[^1]),
                labels_cond[^1],
            });

        if (local_value_exist)
        {
            /*
                var a = ... ->
                    $local.a = ...
            */
            body.Body
                .Where(x => x is Code code && code.Operator == Operator.Bind && code.Return is VariableValue)
                .OfType<Code>()
                .Each(x => x.Return = new PropertyValue(_local, x.Return!.Cast<VariableValue>().Name));
        }

        next_body.Body.AddRange(body.Body.SplitBefore(IsYield).Select((chunk, i) =>
        {
            if (i == 0) return local_value_exist ? ConvertVariableToCoroutineProperty(chunk, _local, body.LexicalScope) : chunk;

            var yield_line = chunk.First().Cast<Call>();

            var yield_block = new List<IOperand>() {
                    new Code { Operator = Operator.Bind, Return = new PropertyValue(_self, "value"), Left =  yield_line.Function.Arguments[0] },
                    new Call(new FunctionCallValue(co_struct_name)) { Return = _next },
                    new Code { Operator = Operator.Bind, Return = new PropertyValue(_next, "state"), Left = new NumericValue() { Value = (uint)i } },
            };
            if (local_value_exist)
            {
                yield_block.Add(new Code { Operator = Operator.Bind, Return = new PropertyValue(_next, "local"), Left = _local });
            }
            yield_block.AddRange(new IOperand[] {
                    new Code { Operator = Operator.Bind, Return = new PropertyValue(_self, "next"), Left = _next },
                    new Call(new FunctionCallValue(tuple2).Return(x => x.Arguments.AddRange(new IEvaluable[] { yield_line.Function.Arguments[0], _next }))) { Return = _ret },
                    new Call(new FunctionCallValue(new VariableValue() { Name = "return" }).Return(x => x.Arguments.Add(_ret))),
                    labels_jump[i - 1],
            });
            var yield_return_converted = yield_block.Concat(chunk.Skip(1));
            return local_value_exist ? ConvertVariableToCoroutineProperty(yield_return_converted, _local, body.LexicalScope) : yield_return_converted;
        }).Flatten());

        next_body.Body.AddRange(new IOperand[] {
                labels_jump[^1],
                new Call(new FunctionCallValue(new VariableValue() { Name = "-" }).Return(x => x.Arguments.Add(new NumericValue() { Value = 1 }))) { Return = _m1 },
                new Code { Operator = Operator.Bind, Return = new PropertyValue(_self, "state"), Left = _m1 },
                new Call(new FunctionCallValue(tuple2).Return(x => x.Arguments.AddRange(new IEvaluable[] { new NullValue(), _self }))) { Return = _ret },
                new Call(new FunctionCallValue(new VariableValue() { Name = "return" }).Return(x => x.Arguments.Add(_ret))),
            });

        /*
            sub co<List<x, a>>(args...) x
                var $ret = Co$0()
                var $local = CoLocal$0() # local value exist
                $local.args... = args... # arguments exist
                $ret.local = $local
                return($ret)
        */
        body.Body.Clear();
        body.LexicalScope.Clear();
        body.LexicalScope["$ret"] = _ret;
        body.Return = co_struct_typename;
        body.Body.Add(new Call(new FunctionCallValue(new VariableValue() { Name = co_struct.Name })) { Return = _ret });
        if (local_value_exist)
        {
            body.LexicalScope["$local"] = _local;
            body.Body.Add(new Call(new FunctionCallValue(new VariableValue() { Name = co_local.Name })) { Return = _local });

            body.Arguments.Each(x =>
            {
                body.LexicalScope.Add(x.Name.Name, x.Name);
                body.Body.Add(new Code { Operator = Operator.Bind, Return = new PropertyValue(_local, x.Name.Name), Left = x.Name });
            });
            body.Body.Add(new Code { Operator = Operator.Bind, Return = new PropertyValue(_ret, "local"), Left = _local });
        }
        body.Body.Add(new Call(new FunctionCallValue(new VariableValue() { Name = "return" }).Return(x => x.Arguments.Add(_ret))));

        /*
            sub isnull($self: Co$0) Bool
                next($self)
                var $m1 = - 1
                var $state = $self.state
                var $ret = $state == $m1
                return($ret)
        */
        var isnull_body = new FunctionBody(src, "isnull");
        isnull_body.Arguments.Add((_self, co_struct_typename));
        isnull_body.Return = new TypeValue() { Name = "Bool" };
        isnull_body.LexicalScope["$self"] = _self;
        isnull_body.LexicalScope["$m1"] = _m1;
        isnull_body.LexicalScope["$state"] = _state;
        isnull_body.LexicalScope["$ret"] = _ret;
        isnull_body.Body.Add(new Call(new FunctionCallValue(next).Return(x => x.Arguments.Add(_self))));
        isnull_body.Body.Add(new Call(new FunctionCallValue(new VariableValue() { Name = "-" }).Return(x => x.Arguments.Add(new NumericValue() { Value = 1 }))) { Return = _m1 });
        isnull_body.Body.Add(new Code { Operator = Operator.Bind, Return = _state, Left = new PropertyValue(_self, "state") });
        isnull_body.Body.Add(new Call(new FunctionCallValue(new VariableValue() { Name = "==" }).Return(x => x.Arguments.AddRange(new IEvaluable[] { _state, _m1 }))) { Return = _ret });
        isnull_body.Body.Add(new Call(new FunctionCallValue(new VariableValue() { Name = "return" }).Return(x => x.Arguments.Add(_ret))));
        src.Functions.Add(isnull_body);
    }

    public static IEnumerable<IOperand> ConvertVariableToCoroutineProperty(
            IEnumerable<IOperand> ops,
            VariableValue _local,
            Dictionary<string, IEvaluable> lexical_scope
        )
    {
        /*
            a ->
                a = $local.a
        */
        var converted = ops.Select(x => EnumLexicalScopeVariableWithoutReturn(x, lexical_scope)).Flatten().Distinct().ToDictionary(x => x, x => false);
        var newops = new List<IOperand>();
        foreach (var op in ops)
        {
            foreach (var v in EnumLexicalScopeVariableWithoutReturn(op, lexical_scope))
            {
                if (!converted[v])
                {
                    newops.Add(new Code { Operator = Operator.Bind, Return = v, Left = new PropertyValue(_local, v.Name) });
                    converted[v] = true;
                }
            }
            newops.Add(op);
        }
        return newops;
    }

    public static IEnumerable<VariableValue> EnumLexicalScopeVariableWithoutReturn(IOperand op, Dictionary<string, IEvaluable> lexical_scope)
    {
        switch (op)
        {
            case Code x:
                {
                    if (x.Left is VariableValue left && lexical_scope.ContainsKey(left.Name)) yield return left;
                    if (x.Return is VariableValue right && lexical_scope.ContainsKey(right.Name)) yield return right;
                }
                break;

            case Call x:
                {
                    if (x.Function.Function is VariableValue f && lexical_scope.ContainsKey(f.Name)) yield return f;
                    foreach (var arg in x.Function.Arguments)
                    {
                        if (arg is VariableValue v && lexical_scope.ContainsKey(v.Name)) yield return v;
                    }
                }
                break;

            case IfCode x:
                {
                    if (x.Condition is VariableValue cond && lexical_scope.ContainsKey(cond.Name)) yield return cond;
                }
                break;

            case IfCastCode x:
                {
                    if (x.Condition is VariableValue cond && lexical_scope.ContainsKey(cond.Name)) yield return cond;
                }
                break;

        }
    }

    public static bool IsYield(IOperand x) => x is Call call && call.Function.Function is VariableValue name && name.Name == "yield";
}
