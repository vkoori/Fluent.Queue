namespace FluentQueue.Exceptions.RuntimeExceptions;

using FluentQueue.Exceptions;

public class RabbitMqChannelNotCreated : RuntimeException
{
    public RabbitMqChannelNotCreated() : base(message: "RabbitMQ channel not created.")
    { }
}
