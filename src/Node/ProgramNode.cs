﻿using System.Collections.Generic;

namespace Roku.Node
{
    public class ProgramNode : INode, IScopeNode
    {
        public int? LineNumber { get; set; }
        public int? LineColumn { get; set; }
        public List<VariableNode> Uses { get; } = new List<VariableNode>();
        public string FileName { get; set; } = "";
        public List<IStatementNode> Statements { get; } = new List<IStatementNode>();
        public List<FunctionNode> Functions { get; } = new List<FunctionNode>();
    }
}
