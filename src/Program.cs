using Command;
using System;

namespace Roku
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var opt = new Option();
#if DEBUG
            opt.Output = "-";
#endif
            var xs = CommandLine.Run(opt, args);
            if (xs.Length == 0)
            {
                FrontEnd.Compile(Console.In, opt.Output);
            }
            else
            {
                FrontEnd.Compile(xs[0], opt.Output);
            }
        }
    }
}
