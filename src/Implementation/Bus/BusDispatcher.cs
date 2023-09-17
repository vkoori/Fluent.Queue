namespace FluentQueue.Implementation.Bus;

using System;
using FluentQueue.Interfaces.Bus;
using FluentQueue.Interfaces.Queue;
using FluentQueue.Interfaces.Message;
using FluentQueue.Implementation.Connection;
using System.Reflection;
using FluentQueue.Exceptions.RuntimeExceptions;
using FluentQueue.Implementation.Helper;

public class BusDispatcher : IBusDispatcher
{
    private readonly BusConnectionPool _connectionPool;
    private readonly BusScanner _busRequiredTypes;
    private readonly IMessage _message;
    private string? _connectionName = null;
    private List<IQueue>? _queue = null;
    private DateTime? _availableAt = null;
    private Dictionary<string, Type> _producer = new();

    public BusDispatcher(BusConnectionPool connectionPool, BusScanner busRequiredTypes, IMessage message)
    {
        _connectionPool = connectionPool;
        _busRequiredTypes = busRequiredTypes;
        _message = message;
    }

    public IBusDispatcher OnConnection(string connectionName)
    {
        _connectionName = connectionName;
        return this;
    }

    public IBusDispatcher OnQueue(IQueue queue)
    {
        _queue = new List<IQueue> { queue };
        return this;
    }

    public IBusDispatcher OnQueue(List<IQueue> queue)
    {
        _queue = queue;
        return this;
    }

    public IBusDispatcher OnDelay(DateTime availableAt)
    {
        _availableAt = availableAt;
        return this;
    }

    public void Dispatch()
    {
        Type typeOfProducer = GetProducer();

        object instanceOfProducer = Activator.CreateInstance(
            type: typeOfProducer,
            args: new object[] { _connectionPool, _connectionName! }
        )!;
        MethodInfo publishMethod = typeOfProducer.GetMethod("Publish")!;

        publishMethod.Invoke(
            obj: instanceOfProducer,
            parameters: new object[] { _message, _queue!, _availableAt! }
        );
    }

    private Type GetProducer()
    {
        string key = _connectionName ?? "_default_";

        if (!_producer.ContainsKey(key))
        {
            List<Type> producersType = _busRequiredTypes.GetProducers();

            Type typeOfConnection = _connectionPool.GetConnection(connectionName: _connectionName).GetType();

            Type? typeOfProducer = producersType.Where(type =>
                type!.BaseType!.GenericTypeArguments[0].IsAssignableFrom(typeOfConnection)
            ).FirstOrDefault();

            if (typeOfProducer == null)
            {
                throw new ProducerNotFound();
            }

            _producer[key] = typeOfProducer;
        }

        return _producer[key];
    }
}
