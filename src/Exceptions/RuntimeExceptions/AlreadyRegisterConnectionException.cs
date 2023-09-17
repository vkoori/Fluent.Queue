namespace FluentQueue.Exceptions.RuntimeExceptions;

using FluentQueue.Exceptions;

public class AlreadyRegisterConnectionException : RuntimeException
{
    public AlreadyRegisterConnectionException() : base(message: "The connection is already registered.")
    { }
}
