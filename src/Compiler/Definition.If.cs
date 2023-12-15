using Extensions;
using Roku.Declare;
using Roku.IntermediateCode;
using Roku.Manager;
using Roku.Node;
using System.Linq;

namespace Roku.Compiler;

public static partial class Definition
{
    public static void IfBodyDefinition(ILexicalScope scope, IIfNode if_)
    {
        var else_ = new LabelCode() { Name = "Else" };
        var elseif = if_.ElseIf.Select(x => new LabelCode() { Name = "ElseIf" }).ToArray();
        var endif = new LabelCode() { Name = "EndIf" };

        var next_label =
            elseif.Length > 0 ? elseif.First()
            : if_.Else is { } ? else_
            : endif;
        IfThenDefinition(scope, if_, next_label);

        if_.ElseIf.Each((x, i) =>
        {
            scope.Body.Add(new GotoCode(endif));
            scope.Body.Add(elseif[i]);

            var next_label =
                elseif.Length > i + 1 ? elseif[i + 1]
                : if_.Else is { } ? else_
                : endif;

            IfThenDefinition(scope, x, next_label);
        });

        if (if_.Else is { })
        {
            scope.Body.Add(new GotoCode(endif));
            scope.Body.Add(else_);
            FunctionBodyDefinition(scope, if_.Else.Statements);
        }
        scope.Body.Add(endif);
    }

    public static void IfThenDefinition(ILexicalScope scope, IIfNode if_, LabelCode next_label)
    {
        var inner_scope = new InnerScope(scope);

        if (if_ is IfNode ifn)
        {
            inner_scope.Body.Add(new IfCode(NormalizationExpression(inner_scope, ifn.Condition, true), next_label));
        }
        else if (if_ is IfCastNode ifc)
        {
            var ifcast = new IfCastCode(new VariableValue { Name = ifc.Name.Name }, CreateType(inner_scope, ifc.Declare), NormalizationExpression(inner_scope, ifc.Condition, true), next_label);
            inner_scope.Body.Add(ifcast);
            inner_scope.LexicalScope.Add(ifc.Name.Name, ifcast.Name);
        }
        else
        {
            var ifa = if_.Cast<IfArrayCastNode>();
            var cond = NormalizationExpression(inner_scope, ifa.Condition, true);
            var _v0 = CreateTemporaryVariable(inner_scope);
            inner_scope.Body.Add(new Call(new FunctionCallValue { Function = new VariableValue { Name = "isnull" } }.Return(x => x.Arguments.Add(cond))) { Return = _v0 });

            switch (ifa.ArrayPattern.List.Count)
            {
                case 0:
                    {
                        /*
                            if [] = cond ->
                                var $0 = isnull(cond)
                                if $0 else goto next
                        */
                        inner_scope.Body.AddRange(new IOperand[] { new IfCode(_v0, next_label), });
                    }
                    break;

                case 1:
                    {
                        /*
                            if [y] = cond ->
                                var $0 = isnull(cond)
                                var $1 = ! $0
                                if $1 else goto next
                                var $2 = next(cond)
                                var $3 = $2.2
                                var $4 = isnull($3)
                                if $4 else goto next
                                var y = $2.1
                        */
                        var _v1 = CreateTemporaryVariable(inner_scope);
                        var _v2 = CreateTemporaryVariable(inner_scope);
                        var _v3 = CreateTemporaryVariable(inner_scope);
                        var _v4 = CreateTemporaryVariable(inner_scope);
                        var y = new VariableValue { Name = ifa.ArrayPattern.List[0].Name };
                        inner_scope.LexicalScope.Add(y.Name, y);
                        inner_scope.Body.AddRange(new IOperand[] {
                                new Call(new FunctionCallValue { Function = new VariableValue { Name = "!" } }.Return(x => x.Arguments.Add(_v0))) { Return = _v1 },
                                new IfCode(_v1, next_label),
                                new Call(new FunctionCallValue { Function = new VariableValue { Name = "next" } }.Return(x => x.Arguments.Add(cond))) { Return = _v2 },
                                new BindCode{ Return = _v3, Value = new PropertyValue { Left =_v2, Right = "2" } },
                                new Call(new FunctionCallValue { Function = new VariableValue { Name = "isnull" } }.Return(x => x.Arguments.Add(_v3))) { Return = _v4 },
                                new IfCode(_v4, next_label),
                                new BindCode{ Return = y, Value = new PropertyValue{ Left = _v2, Right = "1" } },
                            });
                    }
                    break;

                case >= 2:
                    {
                        /*
                            if [y1, y2, ..., y[n - 1], y[n], ys] = cond ->
                                var $0 = isnull(cond)
                                var $1 = ! $0
                                if $1 else goto next

                                # y1 .. y[n - 1]
                                var $[index * 4 + 0] = next($[(index - 1) * 4 + 1])
                                var y[index] = $[index * 4 + 0].1
                                var $[index * 4 + 1] = $[index * 4 + 0].2
                                var $[index * 4 + 2] = isnull($[index * 4 + 1])
                                var $[index * 4 + 3] = ! $[index * 4 + 2]
                                if $[index * 4 + 3] else goto next

                                # y[n]
                                var $[index * 4 + 0] = next($[(index - 1) * 4 + 1])
                                var y[index] = $[index * 4 + 0].1
                                var ys = $[index * 4 + 0].2

                        */
                        var _v1 = CreateTemporaryVariable(inner_scope);
                        inner_scope.Body.AddRange(new IOperand[] {
                                new Call(new FunctionCallValue{ Function = new VariableValue { Name = "!" } }.Return(x => x.Arguments.Add(_v0))) { Return = _v1 },
                                new IfCode(_v1, next_label),
                            });
                        var prev_cond = cond;
                        for (var index = 0; index < ifa.ArrayPattern.List.Count - 2; index++)
                        {
                            var _v4_x_index_add_0 = CreateTemporaryVariable(inner_scope);
                            var _v4_x_index_add_1 = CreateTemporaryVariable(inner_scope);
                            var _v4_x_index_add_2 = CreateTemporaryVariable(inner_scope);
                            var _v4_x_index_add_3 = CreateTemporaryVariable(inner_scope);
                            var y = new VariableValue { Name = ifa.ArrayPattern.List[index].Name };
                            inner_scope.LexicalScope.Add(y.Name, y);
                            inner_scope.Body.AddRange(new IOperand[] {
                                    new Call(new FunctionCallValue{ Function = new VariableValue { Name = "next" } }.Return(x => x.Arguments.Add(prev_cond))) { Return = _v4_x_index_add_0 },
                                    new BindCode{ Return = y, Value = new PropertyValue{ Left = _v4_x_index_add_0, Right = "1" } },
                                    new BindCode{ Return = _v4_x_index_add_1, Value = new PropertyValue{ Left = _v4_x_index_add_0, Right = "2" } },
                                    new Call(new FunctionCallValue{ Function = new VariableValue { Name = "isnull" } }.Return(x => x.Arguments.Add(_v4_x_index_add_1))) { Return = _v4_x_index_add_2 },
                                    new Call(new FunctionCallValue{ Function = new VariableValue { Name = "!" } }.Return(x => x.Arguments.Add(_v4_x_index_add_2))) { Return = _v4_x_index_add_3 },
                                    new IfCode(_v4_x_index_add_3, next_label),
                                });
                            prev_cond = _v4_x_index_add_1;
                        }
                        var _v4_x_last_add_0 = CreateTemporaryVariable(inner_scope);
                        var yn = new VariableValue { Name = ifa.ArrayPattern.List[ifa.ArrayPattern.List.Count - 2].Name };
                        var ys = new VariableValue { Name = ifa.ArrayPattern.List[ifa.ArrayPattern.List.Count - 1].Name };
                        inner_scope.LexicalScope.Add(yn.Name, yn);
                        inner_scope.LexicalScope.Add(ys.Name, ys);
                        inner_scope.Body.AddRange(new IOperand[] {
                                new Call(new FunctionCallValue{ Function = new VariableValue { Name = "next" } }.Return(x => x.Arguments.Add(prev_cond))) { Return = _v4_x_last_add_0 },
                                new BindCode{ Return = yn, Value = new PropertyValue{ Left = _v4_x_last_add_0, Right = "1" } },
                                new BindCode{ Return = ys, Value = new PropertyValue{ Left = _v4_x_last_add_0, Right = "2" } },
                            });
                    }
                    break;
            }
        }

        FunctionBodyDefinition(inner_scope, if_.Then.Statements);
        scope.Body.AddRange(inner_scope.Body);
        scope.MaxTemporaryValue = inner_scope.MaxTemporaryValue;
    }
}
