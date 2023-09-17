namespace FluentQueue.Implementation.Drivers.RabbitMq.Bus.Extensions;

using FluentQueue.Interfaces.Drivers.RabbitMQ.Message;
using FluentQueue.Interfaces.Drivers.RabbitMQ.Queue;
using FluentQueue.Implementation.Drivers.RabbitMq.Queue;
using RabbitMQ.Client;

public static class RabbitMqChannel
{
    public static void HardClose(this IModel channel)
    {
        channel.Close();
        channel.Dispose();
    }

    public static List<string> CreateQueue(this IModel channel, List<IRabbitMqQueue>? queues)
    {
        if (queues == null)
        {
            queues = DefaultQueues();
        }

        List<string> queueNames = new();

        foreach (IRabbitMqQueue queue in queues)
        {
            queueNames.Add(
                item: CreateQueue(channel: channel, queue: queue)
            );
        }

        return queueNames;
    }

    public static void ManageExchange(this IModel channel, IExchange exchange, List<string> queueBindings, string? routingKey)
    {
        CreateExchange(channel: channel, exchange: exchange);
        foreach (string queueName in queueBindings)
        {
            BindExchangeToQueue(channel: channel, exchange: exchange, queueName: queueName, routingKey: routingKey);
        }
    }

    private static string CreateQueue(IModel channel, IRabbitMqQueue queue)
    {
        channel.QueueDeclare(
            queue: queue.QueueName,
            durable: queue.Durable,
            exclusive: queue.Exclusive,
            autoDelete: queue.AutoDelete,
            arguments: queue.Arguments
        );

        return queue.QueueName;
    }

    private static List<IRabbitMqQueue> DefaultQueues()
    {
        return new List<IRabbitMqQueue> {
            new RabbitMqDefaultQueue()
        };
    }

    private static void CreateExchange(this IModel channel, IExchange exchange)
    {
        channel.ExchangeDeclare(
            exchange: exchange.Name,
            type: exchange.Type
        );
    }

    private static void BindExchangeToQueue(this IModel channel, IExchange exchange, string queueName, string? routingKey)
    {
        channel.QueueBind(
            queue: queueName,
            exchange: exchange.Name,
            routingKey: routingKey == null ? string.Empty : routingKey
        );
    }
}
