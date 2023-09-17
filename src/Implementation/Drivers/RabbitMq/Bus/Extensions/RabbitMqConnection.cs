namespace FluentQueue.Implementation.Drivers.RabbitMq.Bus.Extensions;

using RabbitMQ.Client;
using FluentQueue.Exceptions.RuntimeExceptions;

public static class RabbitMqConnection
{
    public static IModel CreateChannel(this IConnection connection)
    {
        IModel? channel = connection.CreateModel();
        if (channel == null)
        {
            throw new RabbitMqChannelNotCreated();
        }
        return channel;
    }
}
