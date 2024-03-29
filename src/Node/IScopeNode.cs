﻿using System.Collections.Generic;

namespace Roku.Node;

public interface IScopeNode : INode
{
    public List<IStatementNode> Statements { get; }
    public List<FunctionNode> Functions { get; }
    public List<StructNode> Structs { get; }
    public List<ClassNode> Classes { get; }
    public List<InstanceNode> Instances { get; }
}
