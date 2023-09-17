namespace FluentQueue.Exceptions;

using System;

public class RuntimeException : Exception
{
    public RuntimeException(string message) : base(message: message)
    { }
}
