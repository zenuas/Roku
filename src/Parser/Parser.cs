

using Extensions;
using Roku.Node;


using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Roku.Parser
{
    public partial class Parser
    {
        public List<Token> TokenStack { get; } = new List<Token>();
        public int[,] Tables { get; } = new int[,] {
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -50, 0, 0, 0, 0, 0, 0, -50, 0, 0, -50, -50, 0, 0, 0, 0, -50, -50, 0, 0, -50, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 65, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -4, 0, 0, 0, 0, 0, 0, -4, 0, 0, -4, -4, 0, 0, 0, 0, -4, -4, 0, 0, -4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -5, 0, 0, 0, 0, 0, 0, -5, 0, 0, -5, -5, 0, 0, 0, 0, -5, -5, 0, 0, -5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -6, 0, 0, 0, 0, 0, 0, -6, 0, 0, -6, -6, 0, 0, 0, 0, -6, -6, 0, 0, -6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -7, 0, 0, 0, 0, 0, 0, -7, 0, 0, -7, -7, 0, 0, 0, 0, -7, -7, 0, 0, -7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -8, 0, 0, 0, 0, 0, 0, -8, 0, 0, -8, -8, 0, 0, 0, 0, -8, -8, 0, 0, -8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -9, 0, 0, 0, 0, 0, 0, -9, 0, 0, -9, -9, 0, 0, 0, 0, -9, -9, 0, 0, -9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-10, -10, -10, 0, 0, 0, 0, 0, 0, 0, 0, 0, -10, 0, 0, 0, 0, 0, 0, 0, 0, -10, -10, -10, 0, -10, 0, -10, 0, 0, 0, -10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-12, -12, -12, 0, 0, 0, 0, 0, 0, 0, 0, 0, -12, 0, 0, 0, 0, 0, 0, 0, 0, -12, -12, -12, 0, -12, 0, -12, 0, 0, 0, -12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-13, -13, -13, 0, 0, 0, 0, 0, 0, 0, 0, 0, -13, 0, 0, 0, 0, 0, 0, 0, 0, -13, -13, -13, 0, -13, 0, -13, 0, 0, 0, -13, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 58, 15, 0, 0, 0, 0, 59, 0, 0, 0, 54, 0, 0, 0, 14, 0, 69, 0, 0, 0, 0, 0, 0, 0, 0, 0, 13, 0, 0, 0, 68, 0, 0, 0, 0, 0, 0, 12, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 58, 15, 0, 0, 0, 0, 59, 0, 0, 0, 54, 0, 0, 0, 14, 0, 70, 0, 0, 0, 0, 0, 0, 0, 0, 0, 13, 0, 0, 0, 68, 0, 0, 0, 0, 0, 0, 12, 0, 0, 0},
                {0, -50, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 58, 15, 0, 0, 0, 0, 59, 0, 0, 0, 54, 0, 0, 0, 14, 0, 72, 0, 0, 0, 0, 18, 71, 21, 0, 0, 13, 0, 0, 0, 68, 0, 0, 0, 0, 0, 0, 12, 0, 20, 0},
                {0, 19, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-16, -16, -16, 0, 0, 0, 0, 0, 0, 0, 0, 0, -16, 0, 0, 0, 0, 0, 0, 0, 0, -16, -16, -16, 0, -16, 0, -16, 0, 0, 0, -16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, -17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-50, -50, 64, 0, 0, 0, 0, 0, 0, 0, 0, -50, -50, 0, 0, 0, 0, 0, -50, 0, 0, -50, -50, 0, 0, 0, 0, -50, -50, 0, 0, -50, 0, 0, 0, 0, 0, 0, 22, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 63, 0},
                {0, -18, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 58, 15, 0, 0, 0, 0, 59, 0, 0, 0, 54, 0, 0, 0, 14, 0, 73, 0, 0, 0, 0, 0, 0, 0, 0, 0, 13, 0, 0, 0, 68, 0, 0, 0, 0, 0, 0, 12, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 58, 15, 0, 0, 0, 0, 59, 0, 0, 0, 54, 0, 0, 0, 14, 0, 74, 0, 0, 0, 0, 0, 0, 0, 0, 0, 13, 0, 0, 0, 68, 0, 0, 0, 0, 0, 0, 12, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 26, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 27, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 58, 15, 0, 0, 0, 0, 59, 0, 0, 0, 54, 0, 0, 0, 14, 0, 75, 0, 0, 0, 0, 0, 0, 0, 0, 0, 13, 0, 0, 0, 68, 0, 0, 0, 0, 0, 0, 12, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 0, 0, 0, 0, 0, 29, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 40, 0, 0, 0},
                {-50, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 41, 30},
                {31, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, -50, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 54, 0, 76, 32, 0, 44, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 46, 0, 42, 0},
                {0, 33, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -50, 0, 0, 0, 0, 0, 57, 0, 0, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 54, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 50, 0, 0, 0, 0, 0, 0, 0, 0, 53, 49, 34, 55, 51, 52, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 35, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 39, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 37, 36, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -24, 0, 0, 0, 0, 0, 0, -24, 0, 0, -24, -24, 0, 0, 0, 0, -24, -24, 0, 0, -24, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -50, 0, 0, 0, 0, 0, 0, -50, 0, 0, -50, -50, 0, 0, 0, 0, -50, -50, 0, 0, -50, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 77, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -25, 0, 0, 0, 0, 0, 0, -25, 0, 0, -25, -25, 0, 0, 0, 0, -25, -25, 0, 0, -25, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -26, 0, 0, 0, 0, 0, 0, -26, 0, 0, -26, -26, 0, 0, 0, 0, -26, -26, 0, 0, -26, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-27, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-28, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, -29, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, -30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, -31, -31, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, -32, -32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 47, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 57, 0, 0, 0, 0, 0, 0, 0, 0, 0, 56, 0, 0, 54, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 50, 0, 0, 0, 0, 0, 0, 0, 0, 48, 49, 0, 55, 51, 0, 0},
                {0, -33, -33, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, -34, -34, 0, 0, 0, 0, 0, 0, 0, 0, 0, -34, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, -35, -35, 0, 0, 0, 0, 0, 0, 0, 0, 0, -35, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, -36, -36, 0, 0, 0, 0, 0, 0, 0, 0, 0, -36, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -37, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -38, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-39, -39, -39, 0, -39, 0, 0, 0, 0, 0, 0, 0, -39, -39, 0, 0, 0, 0, 0, 0, 0, -39, -39, -39, 0, -39, 0, -39, 0, 0, 0, -39, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, -40, -40, 0, 0, 0, 0, 0, 0, 0, 0, 0, -40, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, -41, -41, 0, 0, 0, 0, 0, 0, 0, 0, 0, -41, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, -42, -42, 0, 0, 0, 0, 0, 0, 0, 0, 0, -42, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-43, -43, -43, 0, 0, 0, 0, 0, 0, 0, 0, 0, -43, 0, 0, 0, 0, 0, 0, 0, 0, -43, -43, -43, 0, -43, 0, -43, 0, 0, 0, -43, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-44, -44, -44, 0, 0, 0, 0, 0, 0, 0, 0, 0, -44, 0, 0, 0, 0, 0, 0, 0, 0, -44, -44, -44, 0, -44, 0, -44, 0, 0, 0, -44, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-45, -45, -45, 0, 0, 0, 0, 0, 0, 0, 0, 0, -45, 0, 0, 0, 0, 0, 0, 0, 0, -45, -45, -45, 0, -45, 0, -45, 0, 0, 0, -45, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-46, -46, -46, 0, 0, 0, 0, 0, 0, 0, 0, 0, -46, 0, 0, 0, 0, 0, 0, 0, 0, -46, -46, -46, 0, -46, 0, -46, 0, 0, 0, -46, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-47, -47, -47, 0, 0, 0, 0, 0, 0, 0, 0, 0, -47, 0, 0, 0, 0, 0, 0, 0, 0, -47, -47, -47, 0, -47, 0, -47, 0, 0, 0, -47, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, -48, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, -49, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 25, 0, 0, 58, 15, 0, 0, 0, 0, 59, 28, 0, 0, 54, 0, 0, 0, 66, 0, 67, 0, 0, 9, 7, 0, 0, 0, 0, 0, 13, 0, 0, 0, 68, 11, 0, 0, 0, 0, 0, 12, 0, 0, 0},
                {-13, -13, -13, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 0, -13, -13, -13, 0, -13, 0, -13, 0, 0, 0, -13, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 61, 0, 62, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-11, -11, -11, 0, 0, 0, 0, 0, 0, 0, 0, 0, -11, 0, 0, 0, 0, 0, 0, 0, 0, -11, -11, -11, 0, -11, 0, 60, 0, 0, 0, -11, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {17, -14, -14, 0, 0, 0, 0, 0, 0, 0, 0, 0, -14, 0, 0, 0, 0, 0, 0, 0, 0, -14, -14, -14, 0, -14, 0, -14, 0, 0, 0, -14, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {17, -15, -15, 0, 0, 0, 0, 0, 0, 0, 0, 0, -15, 0, 0, 0, 0, 0, 0, 0, 0, -15, -15, -15, 0, -15, 0, -15, 0, 0, 0, -15, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, -20, 24, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {17, -19, 23, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 61, 0, 62, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {17, -21, -21, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 61, 0, 62, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {17, -22, -22, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 61, 0, 62, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -23, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 61, 0, 62, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {-50, -50, 78, 0, 0, 0, 0, 0, 0, 0, 0, -50, -50, 0, 0, 0, 0, 0, -50, 0, 0, -50, -50, 0, 0, 0, 0, -50, -50, 0, 0, -50, 0, 0, 0, 0, 0, 0, 43, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 63, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 38, 0, 0, 0, 0, 0, 0, 25, 0, 0, 58, 15, 0, 0, 0, 0, 59, 28, 0, 0, 54, 0, 0, 0, 66, 0, 67, 0, 0, 9, 7, 0, 0, 0, 0, 0, 13, 0, 0, 0, 68, 11, 0, 0, 0, 0, 0, 12, 0, 0, 0},
                {0, -49, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 0, 0, 45, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 46, 0, 0, 0},
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
                    yy_value = ((IScopeNode)GetValue(-2)).Return(x => { if (((IStatementNode)GetValue(-1)) is { }) x.Statements.Add(((IStatementNode)GetValue(-1))); });
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
                    TraceAction("line : sub");
                    Scopes.Peek().Functions.Add(((FunctionNode)GetValue(-1)));
                    yy_token = DoAction(Symbols.line, 1, yy_value);
                    break;

                case -10:
                    TraceAction("expr : var");
                    yy_value = DefaultAction(1);
                    yy_token = DoAction(Symbols.expr, 1, yy_value);
                    break;

                case -11:
                    TraceAction("expr : str");
                    yy_value = DefaultAction(1);
                    yy_token = DoAction(Symbols.expr, 1, yy_value);
                    break;

                case -12:
                    TraceAction("expr : num");
                    yy_value = DefaultAction(1);
                    yy_token = DoAction(Symbols.expr, 1, yy_value);
                    break;

                case -13:
                    TraceAction("expr : call");
                    yy_value = DefaultAction(1);
                    yy_token = DoAction(Symbols.expr, 1, yy_value);
                    break;

                case -14:
                    TraceAction("expr : ope expr");
                    yy_value = CreateFunctionCallNode(((TokenNode)GetValue(-2)), ((IEvaluableNode)GetValue(-1)));
                    yy_token = DoAction(Symbols.expr, 2, yy_value);
                    break;

                case -15:
                    TraceAction("expr : expr nope expr");
                    yy_value = CreateFunctionCallNode(((TokenNode)GetValue(-2)), ((IEvaluableNode)GetValue(-3)), ((IEvaluableNode)GetValue(-1)));
                    yy_token = DoAction(Symbols.expr, 3, yy_value);
                    break;

                case -16:
                    TraceAction("call : expr '(' list ')'");
                    yy_value = CreateFunctionCallNode(((IEvaluableNode)GetValue(-4)), ((ListNode<IEvaluableNode>)GetValue(-2)).List.ToArray());
                    yy_token = DoAction(Symbols.call, 4, yy_value);
                    break;

                case -17:
                    TraceAction("list : void");
                    yy_value = DefaultAction(1);
                    yy_token = DoAction(Symbols.list, 1, yy_value);
                    break;

                case -18:
                    TraceAction("list : listn extra");
                    yy_value = DefaultAction(2);
                    yy_token = DoAction(Symbols.list, 2, yy_value);
                    break;

                case -19:
                    TraceAction("listn : expr");
                    yy_value = CreateListNode(((IEvaluableNode)GetValue(-1)));
                    yy_token = DoAction(Symbols.listn, 1, yy_value);
                    break;

                case -20:
                    TraceAction("listn : list2n");
                    yy_value = DefaultAction(1);
                    yy_token = DoAction(Symbols.listn, 1, yy_value);
                    break;

                case -21:
                    TraceAction("list2n : expr ',' expr");
                    yy_value = CreateListNode(((IEvaluableNode)GetValue(-3)), ((IEvaluableNode)GetValue(-1)));
                    yy_token = DoAction(Symbols.list2n, 3, yy_value);
                    break;

                case -22:
                    TraceAction("list2n : list2n ',' expr");
                    yy_value = ((ListNode<IEvaluableNode>)GetValue(-3)).Return(x => x.List.Add(((IEvaluableNode)GetValue(-1))));
                    yy_token = DoAction(Symbols.list2n, 3, yy_value);
                    break;

                case -23:
                    TraceAction("let : LET var EQ expr");
                    yy_value = CreateLetNode(((VariableNode)GetValue(-3)), ((IEvaluableNode)GetValue(-1)));
                    yy_token = DoAction(Symbols.let, 4, yy_value);
                    break;

                case -24:
                    TraceAction("sub : SUB fn where '(' args ')' typex EOL sub_block");
                    yy_value = CreateFunctionNode(((FunctionNode)GetValue(-1)), ((VariableNode)GetValue(-8)), ((ListNode<DeclareNode>)GetValue(-5)), ((TypeNode?)GetValue(-3)), (GetValue(-7)));
                    yy_token = DoAction(Symbols.sub, 9, yy_value);
                    break;

                case -25:
                    TraceAction("sub_block : sub_begin stmt END");
                    yy_value = Scopes.Pop();
                    yy_token = DoAction(Symbols.sub_block, 3, yy_value);
                    break;

                case -26:
                    TraceAction("sub_begin : BEGIN");
                    Scopes.Push(new FunctionNode { LineNumber = (GetToken(-1)).LineNumber });
                    yy_token = DoAction(Symbols.sub_begin, 1, yy_value);
                    break;

                case -27:
                    TraceAction("fn : var");
                    yy_value = DefaultAction(1);
                    yy_token = DoAction(Symbols.fn, 1, yy_value);
                    break;

                case -28:
                    TraceAction("where : void");
                    yy_value = DefaultAction(1);
                    yy_token = DoAction(Symbols.where, 1, yy_value);
                    break;

                case -29:
                    TraceAction("args : void");
                    yy_value = CreateListNode<DeclareNode>();
                    yy_token = DoAction(Symbols.args, 1, yy_value);
                    break;

                case -30:
                    TraceAction("args : argn extra");
                    yy_value = DefaultAction(2);
                    yy_token = DoAction(Symbols.args, 2, yy_value);
                    break;

                case -31:
                    TraceAction("argn : decla");
                    yy_value = CreateListNode(((DeclareNode)GetValue(-1)));
                    yy_token = DoAction(Symbols.argn, 1, yy_value);
                    break;

                case -32:
                    TraceAction("argn : argn ',' decla");
                    yy_value = ((ListNode<DeclareNode>)GetValue(-3)).Return(x => x.List.Add(((DeclareNode)GetValue(-1))));
                    yy_token = DoAction(Symbols.argn, 3, yy_value);
                    break;

                case -33:
                    TraceAction("decla : var ':' type");
                    yy_value = new DeclareNode(((VariableNode)GetValue(-3)), ((TypeNode)GetValue(-1)));
                    yy_token = DoAction(Symbols.decla, 3, yy_value);
                    break;

                case -34:
                    TraceAction("type : typev");
                    yy_value = DefaultAction(1);
                    yy_token = DoAction(Symbols.type, 1, yy_value);
                    break;

                case -35:
                    TraceAction("typev : nsvar");
                    yy_value = DefaultAction(1);
                    yy_token = DoAction(Symbols.typev, 1, yy_value);
                    break;

                case -36:
                    TraceAction("nsvar : varx");
                    yy_value = new TypeNode { Name = ((VariableNode)GetValue(-1)).Name }.R(((VariableNode)GetValue(-1)));
                    yy_token = DoAction(Symbols.nsvar, 1, yy_value);
                    break;

                case -37:
                    TraceAction("typex : void");
                    yy_value = DefaultAction(1);
                    yy_token = DoAction(Symbols.typex, 1, yy_value);
                    break;

                case -38:
                    TraceAction("typex : type");
                    yy_value = DefaultAction(1);
                    yy_token = DoAction(Symbols.typex, 1, yy_value);
                    break;

                case -39:
                    TraceAction("var : VAR");
                    yy_value = CreateVariableNode((GetToken(-1)));
                    yy_token = DoAction(Symbols.var, 1, yy_value);
                    break;

                case -40:
                    TraceAction("varx : var");
                    yy_value = DefaultAction(1);
                    yy_token = DoAction(Symbols.varx, 1, yy_value);
                    break;

                case -41:
                    TraceAction("varx : SUB");
                    yy_value = CreateVariableNode((GetToken(-1)));
                    yy_token = DoAction(Symbols.varx, 1, yy_value);
                    break;

                case -42:
                    TraceAction("varx : LET");
                    yy_value = CreateVariableNode((GetToken(-1)));
                    yy_token = DoAction(Symbols.varx, 1, yy_value);
                    break;

                case -43:
                    TraceAction("num : NUM");
                    yy_value = ((NumericNode)GetValue(-1));
                    yy_token = DoAction(Symbols.num, 1, yy_value);
                    break;

                case -44:
                    TraceAction("str : STR");
                    yy_value = new StringNode { Value = (GetToken(-1)).Name }.R((GetToken(-1)));
                    yy_token = DoAction(Symbols.str, 1, yy_value);
                    break;

                case -45:
                    TraceAction("str : str STR");
                    yy_value = ((StringNode)GetValue(-2)).Return(x => x.Value += (GetToken(-1)).Name);
                    yy_token = DoAction(Symbols.str, 2, yy_value);
                    break;

                case -46:
                    TraceAction("nope : OPE");
                    yy_value = new TokenNode { Token = (GetToken(-1)) }.R((GetToken(-1)));
                    yy_token = DoAction(Symbols.nope, 1, yy_value);
                    break;

                case -47:
                    TraceAction("nope : OR");
                    yy_value = new TokenNode { Token = (GetToken(-1)) }.R((GetToken(-1)));
                    yy_token = DoAction(Symbols.nope, 1, yy_value);
                    break;

                case -48:
                    TraceAction("extra : void");
                    yy_value = DefaultAction(1);
                    yy_token = DoAction(Symbols.extra, 1, yy_value);
                    break;

                case -49:
                    TraceAction("extra : ','");
                    yy_value = DefaultAction(1);
                    yy_token = DoAction(Symbols.extra, 1, yy_value);
                    break;

                case -50:
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
