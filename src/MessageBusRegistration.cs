namespace FluentQueue;

using FluentQueue.Exceptions.RuntimeExceptions;
using FluentQueue.Interfaces.Bus;
using FluentQueue.Interfaces.Queue;
using FluentQueue.Implementation.Bus;
using FluentQueue.Implementation.Connection;
using FluentQueue.Implementation.Helper;
using Microsoft.Extensions.DependencyInjection;

public static class MessageBusRegistration
{
    public static IServiceCollection AddMessageBus(
        this IServiceCollection services,
        Dictionary<string, BaseBusConnectionDtoAbstract> connections,
        string defaultConnection
    )
    {
        BusScanner busScanner = new();
        busScanner.ExtractRequireTypes();

        services.AddSingleton(sp => busScanner);

        services.AddSingleton(sp =>
        {
            BusConnectionPool connectionPool = new(busRequiredTypes: busScanner);
            foreach (var connection in connections)
            {
                connectionPool.AddConnection(connectionName: connection.Key, connectionDto: connection.Value);
            }
            connectionPool.SetDefaultConnection(connectionName: defaultConnection);
            return connectionPool;
        });

        services.AddScoped<IBus, Bus>();

        return services;
    }

    public static IServiceCollection AddSubscriberBus(
        this IServiceCollection services,
        List<IQueue> queues,
        string? connectionName = null,
        ushort retry = 3,
        ushort consumerCount = 1
    )
    {
        for (int i = 0; i < consumerCount; i++)
        {
            services.AddHostedService(provider =>
            {
                BusConnectionPool busConnectionPool = provider.GetServices<BusConnectionPool>().FirstOrDefault() ?? throw new BusNotRegistered();
                BusScanner busScanner = provider.GetServices<BusScanner>().FirstOrDefault() ?? throw new BusNotRegistered();
                return new BusTracker(
                    busRequiredTypes: busScanner,
                    connectionPool: busConnectionPool,
                    connectionName: connectionName,
                    queues: queues,
                    retry: retry
                );
            });
        }

        return services;
    }
}
