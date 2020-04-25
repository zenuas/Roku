

using Extensions;
using Roku.Node;
using IEvaluableListNode = Roku.Node.ListNode<Roku.Node.IEvaluableNode>;


using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Roku.Parser
{
    public partial class Parser
    {
        public List<Token> TokenStack { get; } = new List<Token>();
        public int[,] Tables { get; } = new int[,] {
                {0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -2, 0, 0, 0, 0, 0, 0, 0, 0, 3, 1, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, -25, 0, 0, 0, 0, 0, 0, -25, 0, -25, -25, 0, 0, 0, -25, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 30, 0, 0, 6},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, -4, 0, 0, 0, 0, 0, 0, -4, 0, -4, -4, 0, 0, 0, -4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, -5, 0, 0, 0, 0, 0, 0, -5, 0, -5, -5, 0, 0, 0, -5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, -6, 0, 0, 0, 0, 0, 0, -6, 0, -6, -6, 0, 0, 0, -6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, -7, 0, 0, 0, 0, 0, 0, -7, 0, -7, -7, 0, 0, 0, -7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, -8, 0, 0, 0, 0, 0, 0, -8, 0, -8, -8, 0, 0, 0, -8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-9, 0, -9, 0, 0, 0, 0, 0, 0, -9, 0, -9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-11, 0, -11, 0, 0, 0, 0, 0, 0, -11, 0, -11, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-12, 0, -12, 0, 0, 0, 0, 0, 0, -12, 0, -12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {15, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, -25, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 27, 28, 0, 0, 0, 26, 0, 13, 34, 0, 0, 16, 33, 19, 12, 0, 0, 0, 32, 11, 18},
                {0, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-13, 0, -13, 0, 0, 0, 0, 0, 0, -13, 0, -13, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, -14, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, -15, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 27, 28, 0, 0, 0, 26, 0, 13, 35, 0, 0, 0, 0, 0, 12, 0, 0, 0, 32, 11, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 27, 28, 0, 0, 0, 26, 0, 13, 36, 0, 0, 0, 0, 0, 12, 0, 0, 0, 32, 11, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 26, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 24, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 25, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 27, 28, 0, 0, 0, 26, 0, 13, 37, 0, 0, 0, 0, 0, 12, 0, 0, 0, 32, 11, 0},
                {-21, 0, -21, 0, 0, 0, 0, 0, 0, -21, -21, -21, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-22, 0, -22, 0, 0, 0, 0, 0, 0, -22, 0, -22, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-23, 0, -23, 0, 0, 0, 0, 0, 0, -23, 0, -23, 0, 0, 0, 0, 0, 0, -23, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-24, 0, -24, 0, 0, 0, 0, 0, 0, -24, 0, -24, 0, 0, 0, 0, 0, 0, -24, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 23, 0, 27, 28, 0, 0, 0, 26, 0, 31, 14, 9, 7, 0, 0, 0, 12, 0, 0, 0, 32, 11, 0},
                {-12, 0, -12, 0, 0, 0, 0, 0, 0, 8, 0, -12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-10, 0, -10, 0, 0, 0, 0, 0, 0, -10, 0, -10, 0, 0, 0, 0, 0, 0, 29, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 22, 0, 0, 0, 0, 0, 0, 0, 0, -17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {15, 0, 21, 0, 0, 0, 0, 0, 0, 0, 0, -16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {15, 0, -18, 0, 0, 0, 0, 0, 0, 0, 0, -18, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {15, 0, -19, 0, 0, 0, 0, 0, 0, 0, 0, -19, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {15, 0, 0, 0, 0, 0, 0, 0, 0, -20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            };

        public INode Parse(ILexer<INode> lex)
        {
            var current = 0;

            while (true)
            {
                var token = (Token)lex.PeekToken();
                var x = Tables[current, token.InputToken];

                if (x < 0)
                {
                    token = RunAction(x);
                    if (token.IsAccept) return token.Value!;
                    current = TokenStack.Count == 0 ? 0 : TokenStack[^1].TableIndex;
                    x = Tables[current, token.InputToken];

                    token.TableIndex = x;
                    TokenStack.Add(token);
                    current = x;
                }
                else if (x == 0)
                {
                    OnError(lex);
                }
                else
                {
                    _ = lex.ReadToken();
                    token.TableIndex = x;
                    TokenStack.Add(token);
                    current = x;
                }
            }
        }

        public Token RunAction(int yy_no)
        {
            Token? yy_token;
            INode? yy_value = null;

            switch (yy_no)
            {
                case -1:
                    TraceAction("$ACCEPT : start $END");
                    yy_value = DefaultAction(2);
                    yy_token = DoAction(Symbols._ACCEPT, 2, yy_value);
                    break;

                case -2:
                    TraceAction("start :");
                    yy_value = new ProgramNode();
                    yy_token = DoAction(Symbols.start, 0, yy_value);
                    break;

                case -3:
                    TraceAction("start : program_begin stmt END");
                    yy_value = Scopes.Pop();
                    yy_token = DoAction(Symbols.start, 3, yy_value);
                    break;

                case -4:
                    TraceAction("program_begin : BEGIN");
                    Scopes.Push(new ProgramNode());
                    yy_token = DoAction(Symbols.program_begin, 1, yy_value);
                    break;

                case -5:
                    TraceAction("stmt : void");
                    yy_value = Scopes.Peek();
                    yy_token = DoAction(Symbols.stmt, 1, yy_value);
                    break;

                case -6:
                    TraceAction("stmt : stmt line");
                    ((IScopeNode)GetValue(-2)).Statements.Add(((IStatementNode)GetValue(-1)));
                    yy_token = DoAction(Symbols.stmt, 2, yy_value);
                    break;

                case -7:
                    TraceAction("line : call EOL");
                    yy_value = DefaultAction(2);
                    yy_token = DoAction(Symbols.line, 2, yy_value);
                    break;

                case -8:
                    TraceAction("line : let EOL");
                    yy_value = DefaultAction(2);
                    yy_token = DoAction(Symbols.line, 2, yy_value);
                    break;

                case -9:
                    TraceAction("expr : var");
                    yy_value = DefaultAction(1);
                    yy_token = DoAction(Symbols.expr, 1, yy_value);
                    break;

                case -10:
                    TraceAction("expr : str");
                    yy_value = DefaultAction(1);
                    yy_token = DoAction(Symbols.expr, 1, yy_value);
                    break;

                case -11:
                    TraceAction("expr : num");
                    yy_value = DefaultAction(1);
                    yy_token = DoAction(Symbols.expr, 1, yy_value);
                    break;

                case -12:
                    TraceAction("expr : call");
                    yy_value = DefaultAction(1);
                    yy_token = DoAction(Symbols.expr, 1, yy_value);
                    break;

                case -13:
                    TraceAction("call : expr '(' list ')'");
                    yy_value = CreateFunctionCallNode(((IEvaluableNode)GetValue(-4)), ((IEvaluableListNode)GetValue(-2)).List.ToArray());
                    yy_token = DoAction(Symbols.call, 4, yy_value);
                    break;

                case -14:
                    TraceAction("list : void");
                    yy_value = DefaultAction(1);
                    yy_token = DoAction(Symbols.list, 1, yy_value);
                    break;

                case -15:
                    TraceAction("list : listn extra");
                    yy_value = DefaultAction(2);
                    yy_token = DoAction(Symbols.list, 2, yy_value);
                    break;

                case -16:
                    TraceAction("listn : expr");
                    yy_value = CreateListNode(((IEvaluableNode)GetValue(-1)));
                    yy_token = DoAction(Symbols.listn, 1, yy_value);
                    break;

                case -17:
                    TraceAction("listn : list2n");
                    yy_value = DefaultAction(1);
                    yy_token = DoAction(Symbols.listn, 1, yy_value);
                    break;

                case -18:
                    TraceAction("list2n : expr ',' expr");
                    yy_value = CreateListNode(((IEvaluableNode)GetValue(-3)), ((IEvaluableNode)GetValue(-1)));
                    yy_token = DoAction(Symbols.list2n, 3, yy_value);
                    break;

                case -19:
                    TraceAction("list2n : list2n ',' expr");
                    yy_value = ((IEvaluableListNode)GetValue(-3)).Return(x => x.List.Add(((IEvaluableNode)GetValue(-1))));
                    yy_token = DoAction(Symbols.list2n, 3, yy_value);
                    break;

                case -20:
                    TraceAction("let : LET var EQ expr");
                    yy_value = CreateLetNode(((VariableNode)GetValue(-3)), ((IEvaluableNode)GetValue(-1)));
                    yy_token = DoAction(Symbols.let, 4, yy_value);
                    break;

                case -21:
                    TraceAction("var : VAR");
                    yy_value = CreateVariableNode((GetToken(-1)));
                    yy_token = DoAction(Symbols.var, 1, yy_value);
                    break;

                case -22:
                    TraceAction("num : NUM");
                    yy_value = ((NumericNode)GetValue(-1));
                    yy_token = DoAction(Symbols.num, 1, yy_value);
                    break;

                case -23:
                    TraceAction("str : STR");
                    yy_value = new StringNode { Value = (GetToken(-1)).Name }.R((GetToken(-1)));
                    yy_token = DoAction(Symbols.str, 1, yy_value);
                    break;

                case -24:
                    TraceAction("str : str STR");
                    yy_value = ((StringNode)GetValue(-2)).Return(x => x.Value += (GetToken(-1)).Name);
                    yy_token = DoAction(Symbols.str, 2, yy_value);
                    break;

                case -25:
                    TraceAction("void :");
                    yy_value = null;
                    yy_token = DoAction(Symbols.@void, 0, yy_value);
                    break;

                default:
                    throw new InvalidProgramException();
            }

            return yy_token;
        }

        public Token GetToken(int from_last_index) => TokenStack[TokenStack.Count + from_last_index];

        public INode GetValue(int from_last_index) => GetToken(from_last_index).Value!;

        public INode DefaultAction(int length) => GetValue(-length);

        public bool IsAccept(Token token) => Tables[TokenStack.Count == 0 ? 0 : TokenStack[^1].TableIndex, token.InputToken] != 0;

        public Token DoAction(Symbols type, int length, INode? value) => DoAction(new Token { Type = type }, length, value);

        public Token DoAction(Token token, int length, INode? value)
        {
            token.Value = value;
            TokenStack.RemoveRange(TokenStack.Count - length, length);
            return token;
        }

        public void OnError(ILexer<INode> lex)
        {
            Debug.Fail("syntax error");
            var t = lex.PeekToken();
            throw new Exception($"syntax error({t.LineNumber}, {t.LineColumn})");
        }

        [Conditional("TRACE")]
        public void TraceAction(string s) => Debug.WriteLine(s);
    }
}
