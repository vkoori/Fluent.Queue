namespace FluentQueue.Implementation.Drivers.RabbitMq.Bus;

using System.Collections.Generic;
using System.Text;
using FluentQueue.Exceptions.RuntimeExceptions;
using FluentQueue.Interfaces.Drivers.RabbitMQ.Queue;
using FluentQueue.Interfaces.Queue;
using FluentQueue.Implementation.Bus;
using FluentQueue.Implementation.Connection;
using FluentQueue.Implementation.Helper;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using FluentQueue.Implementation.Drivers.RabbitMq.Bus.Extensions;

public class RabbitMqSubscriber : SubscriberAbstract<IConnection>
{
    private IConnection _connection;
    private IModel _workerChannel;
    private List<string> _consumerTags = new();

    public RabbitMqSubscriber(
        BusConnectionPool connectionPool,
        string? connectionName,
        ushort? retry,
        BusScanner busRequiredTypes
    ) : base(
        connectionPool,
        connectionName,
        retry,
        busRequiredTypes
    )
    {
        _connection = (IConnection)connectionPool.GetConnection(connectionName: connectionName);
        _workerChannel = _connection.CreateChannel();
    }

    public override void Consuming(List<IQueue>? queues)
    {
        List<IRabbitMqQueue> castedQueues = queues?.ConvertAll<IRabbitMqQueue>(queue => (IRabbitMqQueue)queue) ?? throw new InvalidQueueType();

        if (!_connection.IsOpen)
        {
            _connection = (IConnection)_connectionPool.ConnectionReconnect(connectionName: _connectionName);
        }

        if (_workerChannel.IsClosed)
        {
            StopConsume();
            _workerChannel = _connection.CreateChannel();
        }

        List<string> queueNames = _workerChannel.CreateQueue(queues: castedQueues);

        EventingBasicConsumer consumer = new EventingBasicConsumer(model: _workerChannel);

        consumer.Received += (object? sender, BasicDeliverEventArgs eventArgs) =>
        {
            IBasicProperties properties = eventArgs.BasicProperties;

            byte[] body = eventArgs.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);

            try
            {
                TriggerInvokeJob(message: message, correlationId: properties.CorrelationId);
            }
            catch (System.Exception)
            {
                if(
                    properties.Headers.ContainsKey("attempts") &&
                    (Int32)properties.Headers["attempts"] <= _retry
                )
                {
                    // channel.BasicNack(deliveryTag: eventArgs.DeliveryTag, multiple: false, requeue: true);
                    properties.Headers["attempts"] = (Int32)properties.Headers["attempts"] + 1 ;
                    _workerChannel.BasicPublish(
                        exchange: eventArgs.Exchange,
                        routingKey: eventArgs.RoutingKey,
                        mandatory: true,
                        basicProperties: properties,
                        body: body
                    );
                } else {
                    TriggerFailedJob(message: message, properties: properties);
                }
            }

            // Acknowledge the message
            _workerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
        };

        // Start consuming messages
        foreach (string queueName in queueNames)
        {
            _consumerTags.Add(
                item: _workerChannel.BasicConsume(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer
                )
            );
        }
    }

    public override void StopConsume()
    {
        foreach (string consumerTag in _consumerTags)
        {
            _workerChannel.BasicCancel(consumerTag: consumerTag);
        }
        _workerChannel.HardClose();
        _consumerTags.Clear();
    }
}
