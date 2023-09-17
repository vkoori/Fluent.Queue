namespace FluentQueue.Implementation.Helper;

using System;

public class TriggerJobObj
{
    public required Type Job { get; set; }
    public required object Dto { get; set; }
}
