namespace FluentQueue.Implementation.Bus;

using System;
using FluentQueue.Interfaces.Message;
using FluentQueue.Interfaces.Queue;
using FluentQueue.Implementation.Connection;

public abstract class ProducerAbstract<TConnection, TMessage>
    where TConnection : IDisposable
    where TMessage : IMessage
{
    protected readonly BusConnectionPool _connectionPool;
    protected readonly string? _connectionName;

    public ProducerAbstract(BusConnectionPool connectionPool, string? connectionName)
    {
        _connectionPool = connectionPool;
        _connectionName = connectionName;
    }

    public abstract void Publish(TMessage message, List<IQueue>? queues, DateTime? availableAt);
}
