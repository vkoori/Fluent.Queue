namespace FluentQueue.Exceptions.RuntimeExceptions;

using FluentQueue.Exceptions;

public class NotRegisterConnectionException : RuntimeException
{
    public NotRegisterConnectionException() : base(message: "The connection is not registered.")
    { }
}
