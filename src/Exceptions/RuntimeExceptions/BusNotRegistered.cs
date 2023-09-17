namespace FluentQueue.Exceptions.RuntimeExceptions;

using FluentQueue.Exceptions;

public class BusNotRegistered : RuntimeException
{
    public BusNotRegistered() : base(message: "You have not registered a bus. Please use builder.Services.AddMessageBus()")
    { }
}
