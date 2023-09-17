namespace FluentQueue.Exceptions.RuntimeExceptions;

using FluentQueue.Exceptions;

public class InvalidArgument : RuntimeException
{
    public InvalidArgument() : base(message: "Invalid argument provided. Please check your input and try again.")
    { }

    public InvalidArgument(string argName) : base(message: $"argument {argName} is invalid. Please check your input and try again.")
    { }
}
