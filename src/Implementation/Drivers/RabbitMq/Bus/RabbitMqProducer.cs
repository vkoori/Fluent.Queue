namespace FluentQueue.Implementation.Drivers.RabbitMq.Bus;

using System;
using System.Text;
using FluentQueue.Exceptions.RuntimeExceptions;
using FluentQueue.Interfaces.Drivers.RabbitMQ.Message;
using FluentQueue.Interfaces.Drivers.RabbitMQ.Queue;
using FluentQueue.Interfaces.Message;
using FluentQueue.Interfaces.Queue;
using FluentQueue.Implementation.Bus;
using FluentQueue.Implementation.Connection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using FluentQueue.Implementation.Drivers.RabbitMq.Bus.Extensions;

public class RabbitMqProducer : ProducerAbstract<IConnection, IRabbitMqMessage>
{
    private IConnection _connection;

    public RabbitMqProducer(BusConnectionPool connectionPool, string? connectionName) : base(connectionPool, connectionName)
    {
        _connection = (IConnection)connectionPool.GetConnection(connectionName: connectionName);
    }

    public override void Publish(IRabbitMqMessage message, List<IQueue>? queues, DateTime? availableAt)
    {
        List<IRabbitMqQueue> castedQueues = queues?.ConvertAll<IRabbitMqQueue>(queue => (IRabbitMqQueue)queue) ?? throw new InvalidQueueType();

        if (!_connection.IsOpen)
        {
            _connection = (IConnection)_connectionPool.ConnectionReconnect(connectionName: _connectionName);
        }

        IModel channel = _connection.CreateChannel();

        List<string> queueNames = channel.CreateQueue(queues: castedQueues);

        if (message.Exchange != null)
        {
            channel.ManageExchange(
                exchange: message.Exchange,
                queueBindings: queueNames,
                routingKey: message.RoutingKey
            );
        }

        if (message.RoutingKey == null && message.Exchange == null)
        {
            foreach (string queueName in queueNames)
            {
                message.RoutingKey = queueName;
                PublishMessage(
                    channel: channel,
                    message: message,
                    availableAt: availableAt
                );
            }
        }
        else
        {
            PublishMessage(
                channel: channel,
                message: message,
                availableAt: availableAt
            );
        }

        channel.HardClose();
    }

    protected virtual void PublishMessage(IModel channel, IRabbitMqMessage message, DateTime? availableAt)
    {
        channel.BasicPublish(
            exchange: message.Exchange == null ? string.Empty : message.Exchange.Name,
            routingKey: message.RoutingKey == null ? string.Empty : message.RoutingKey,
            mandatory: true,
            basicProperties: InitProperties(channel: channel, properties: message.Properties, availableAt: availableAt),
            body: InitBody(body: message.Body)
        );
    }

    private IBasicProperties InitProperties(IModel channel, IMessageProperties? properties, DateTime? availableAt)
    {
        IBasicProperties messageProperties = channel.CreateBasicProperties();
        Dictionary<string, object> headers = new();

        if (properties != null)
        {
            if (properties.CorrelationId != null)
            {
                messageProperties.CorrelationId = properties.CorrelationId;
            }
            if (properties.Expiration != null)
            {
                messageProperties.Expiration = Convert.ToInt32(((TimeSpan)(properties.Expiration - DateTime.Now)).TotalMilliseconds).ToString();
            }
            if (availableAt != null)
            {
                headers["x-delay"] = ((TimeSpan)(availableAt - DateTime.Now)).TotalMilliseconds;
            }
        }

        headers["attempts"] = 0;
        messageProperties.Headers = headers;

        return messageProperties;
    }

    private ReadOnlyMemory<byte> InitBody(object body)
    {
        string messageBody = JsonConvert.SerializeObject(body);
        byte[] result = Encoding.UTF8.GetBytes(messageBody);
        return result;
    }
}
