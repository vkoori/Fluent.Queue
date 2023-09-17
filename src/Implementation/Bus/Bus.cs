namespace FluentQueue.Implementation.Bus;

using FluentQueue.Interfaces.Bus;
using FluentQueue.Interfaces.Message;
using FluentQueue.Implementation.Connection;
using FluentQueue.Implementation.Helper;

public class Bus : IBus
{
    private readonly BusConnectionPool _connectionPool;
    private readonly BusScanner _busRequiredTypes;

    public Bus(BusConnectionPool connectionPool, BusScanner busRequiredTypes)
    {
        _connectionPool = connectionPool;
        _busRequiredTypes = busRequiredTypes;
    }

    public IBusDispatcher Message(IMessage message)
    {
        return new BusDispatcher(connectionPool: _connectionPool, busRequiredTypes: _busRequiredTypes, message: message);
    }
}
