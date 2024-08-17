using System.Diagnostics;

namespace Roku.Test;

public class NoFailListener : DefaultTraceListener
{
    public override void Fail(string? message)
    {
    }

    public override void Fail(string? message, string? detailMessage)
    {
    }
}
