namespace FluentQueue.Implementation.Connection;

using System.Collections.Generic;
using System.Reflection;
using FluentQueue.Exceptions.RuntimeExceptions;
using FluentQueue.Implementation.Helper;

public class BusConnectionPool
{
    private readonly BusScanner _busRequiredTypes;
    private string? _defaultConnection = null;
    private Dictionary<string, IDisposable> _connections = new();
    private Dictionary<string, object> _connectionDto = new();

    public BusConnectionPool(BusScanner busRequiredTypes)
    {
        _busRequiredTypes = busRequiredTypes;
    }

    public void AddConnection(string connectionName, object connectionDto)
    {
        if (IsConnectionRegistered(connectionName: connectionName))
        {
            throw new AlreadyRegisterConnectionException();
        }

        _connections[connectionName] = CreateConnection(connectionDto: connectionDto);
        _connectionDto[connectionName] = connectionDto;
    }

    public void SetDefaultConnection(string connectionName)
    {
        if (!IsConnectionRegistered(connectionName: connectionName))
        {
            throw new NotRegisterConnectionException();
        }

        _defaultConnection = connectionName;
    }

    public IDisposable GetConnection(string? connectionName)
    {
        if (connectionName == null)
        {
            return _connections[_defaultConnection ?? ""];
        }

        if (!IsConnectionRegistered(connectionName: connectionName))
        {
            throw new NotRegisterConnectionException();
        }

        return _connections[connectionName];
    }

    public IDisposable ConnectionReconnect(string? connectionName)
    {
        if (connectionName == null)
        {
            connectionName = _defaultConnection ?? "";
        }

        if (!IsConnectionRegistered(connectionName: connectionName))
        {
            throw new NotRegisterConnectionException();
        }

        _connections[connectionName].Dispose();
        _connections[connectionName] = CreateConnection(connectionDto: _connectionDto[connectionName]);

        return _connections[connectionName];
    }

    private bool IsConnectionRegistered(string connectionName)
    {
        return _connections.Keys.Any(key => key == connectionName);
    }

    private IDisposable CreateConnection(object connectionDto)
    {
        Type typeOfConnectionDto = connectionDto.GetType();
        List<Type> connectionsType = _busRequiredTypes.GetConnections();

        Type? typeOfConnectionBuilder = connectionsType.Where(type =>
            type!.BaseType!.GenericTypeArguments[1] == typeOfConnectionDto
        ).FirstOrDefault();

        if (typeOfConnectionBuilder == null)
        {
            throw new InvalidArgument(argName: "connectionDto");
        }

        object instanceOfConnectionBuilder = Activator.CreateInstance(typeOfConnectionBuilder)!;
        MethodInfo createConnectionMethod = typeOfConnectionBuilder.GetMethod("CreateConnection")!;

        return (IDisposable)createConnectionMethod.Invoke(instanceOfConnectionBuilder, new[] { connectionDto })!;
    }
}
