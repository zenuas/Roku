﻿using Mina.Extension;
using System;
using System.IO;
using System.Linq;

namespace Roku.IntermediateCode;

public class ILWriter : IDisposable
{
    public required TextWriter BaseStream { get; init; }
    public int Indent { get; set; } = 0;
    public bool IsLineHead { get; protected set; } = true;

    public void WriteLine() => IsLineHead = WriteLine([""], Indent, IsLineHead);

    public void WriteLine(string? s) => IsLineHead = WriteLine(s!.SplitLine(), Indent, IsLineHead);

    public void Write(string? s) => IsLineHead = Write(s!.SplitLine(), Indent, IsLineHead);

    public bool Write(string[] lines, int indent, bool line_head = true)
    {
        var head = Lists.Repeat(" ").Take(4 * indent).Join("");
        if (lines.Length > 1)
        {
            lines[0..^1].Each((x, i) => BaseStream.WriteLine((i > 0 || line_head ? head : "") + x));
        }
        var last_write = (lines.Length > 1 || (lines.Length == 1 && line_head) ? head : "") + lines[^0];
        BaseStream.Write(last_write);
        return (lines.Length > 1 || (lines.Length == 1 && line_head)) && last_write == "";
    }

    public bool WriteLine(string[] lines, int indent, bool line_head = true)
    {
        lines.Each((x, i) => BaseStream.WriteLine((i > 0 || line_head ? Lists.Repeat(" ").Take(4 * indent).Join("") : "") + x));
        return true;
    }

    public void Dispose() => BaseStream.Flush();
}
