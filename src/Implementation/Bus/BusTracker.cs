namespace FluentQueue.Implementation.Bus;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentQueue.Exceptions.RuntimeExceptions;
using FluentQueue.Interfaces.Queue;
using FluentQueue.Implementation.Connection;
using FluentQueue.Implementation.Helper;
using Microsoft.Extensions.Hosting;

public class BusTracker : IHostedService
{
    private readonly BusScanner _busRequiredTypes;
    private readonly BusConnectionPool _connectionPool;
    private readonly string? _connectionName;
    private readonly List<IQueue> _queues;
    private object _instanceOfSubscriber;
    private MethodInfo _consumingMethod;
    private MethodInfo _stopConsumeMethod;

    public BusTracker(
        BusScanner busRequiredTypes,
        BusConnectionPool connectionPool,
        string? connectionName,
        List<IQueue> queues,
        ushort retry
    )
    {
        _busRequiredTypes = busRequiredTypes;
        _connectionPool = connectionPool;
        _connectionName = connectionName;
        _queues = queues;

        Type typeOfSubscriber = GetSubscriber();

        _instanceOfSubscriber = Activator.CreateInstance(
            type: typeOfSubscriber,
            args: new object[] { connectionPool, connectionName!, retry, busRequiredTypes }
        )!;

        _consumingMethod = typeOfSubscriber.GetMethod("Consuming")!;
        _stopConsumeMethod = typeOfSubscriber.GetMethod("StopConsume")!;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _consumingMethod.Invoke(
            obj: _instanceOfSubscriber,
            parameters: new object[] { _queues }
        );

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _stopConsumeMethod.Invoke(
            obj: _instanceOfSubscriber,
            parameters: new object[] { }
        );

        await Task.CompletedTask;
    }

    private Type GetSubscriber()
    {
        List<Type> subscribersType = _busRequiredTypes.GetSubscribers();

        Type typeOfConnection = _connectionPool.GetConnection(connectionName: _connectionName).GetType();

        Type? typeOfSubscriber = subscribersType.Where(type =>
            type!.BaseType!.GenericTypeArguments[0].IsAssignableFrom(typeOfConnection)
        ).FirstOrDefault();

        if (typeOfSubscriber == null)
        {
            throw new SubscriberNotFound();
        }

        return typeOfSubscriber;
    }
}
