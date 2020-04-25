

using Roku.Node;


using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Roku.Parser
{
    public class Parser
    {
        public List<IToken<INode>> TokenStack { get; } = new List<IToken<INode>>();
        public int[,] Tables { get; } = new int[,] {
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -2, 1},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0},
                {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0},
            };

        public INode? Parse(ILexer<INode> lex)
        {
            var current = 0;

            while (true)
            {
                var token = lex.PeekToken();
                var x = Tables[current, token.InputToken];

                if (x < 0)
                {
                    token = RunAction(x);
                    if (token.IsAccept) return token.Value;
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

        public IToken<INode> RunAction(int yy_no)
        {
            IToken<INode>? yy_token;
            INode? yy_value;

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

                default:
                    throw new InvalidProgramException();
            }

            return yy_token;
        }

        public IToken<INode> GetToken(int from_last_index) => TokenStack[TokenStack.Count + from_last_index];

        public INode? GetValue(int from_last_index) => GetToken(from_last_index).Value;

        public INode DefaultAction(int length) => GetValue(-length)!;

        public bool IsAccept(IToken<INode> token) => Tables[TokenStack.Count == 0 ? 0 : TokenStack[^1].TableIndex, token.InputToken] != 0;

        public IToken<INode> DoAction(Symbols type, int length, INode value) => DoAction(new Token { Type = type }, length, value);

        public IToken<INode> DoAction(IToken<INode> token, int length, INode value)
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
