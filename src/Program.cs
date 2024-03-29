﻿using Mina.Command;
using System;

namespace Roku;

public class Program
{
    public static void Main(string[] args)
    {
        var opt = new Option();
        opt.LoadLibrary("System.Runtime");
        var xs = CommandLine.Run(opt, args);
        if (xs.Length == 0)
        {
            Compiler.FrontEnd.Compile(Console.In, opt.Output, [.. opt.Libraries]);
        }
        else
        {
            Compiler.FrontEnd.Compile(xs[0], opt.Output, [.. opt.Libraries]);
        }
    }
}
