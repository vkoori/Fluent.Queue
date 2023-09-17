namespace FluentQueue.Implementation.Helper;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentQueue.Interfaces.Job;
using FluentQueue.Implementation.Bus;
using FluentQueue.Implementation.Connection;

public class BusScanner

{
    private List<Type> _connections = new();
    private List<Type> _producers = new();
    private List<Type> _subscribers = new();
    private List<Type> _jobs = new();

    public void ExtractRequireTypes()
    {
        _producers = new List<Type>();
        _subscribers = new List<Type>();
        _jobs = new List<Type>();

        Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in loadedAssemblies)
        {
            // if (assembly.GetType().ToString() != "System.Reflection.RuntimeAssembly") break;
            _connections.AddRange(collection: GetConnectionsType(assembly: assembly));
            _producers.AddRange(collection: GetProducersType(assembly: assembly));
            _subscribers.AddRange(collection: GetSubscribersType(assembly: assembly));
            _jobs.AddRange(collection: GetJobsType(assembly: assembly));
        }
    }

    public List<Type> GetConnections()
    {
        return _connections;
    }

    public List<Type> GetProducers()
    {
        return _producers;
    }

    public List<Type> GetSubscribers()
    {
        return _subscribers;
    }

    public List<Type> GetJobs()
    {
        return _jobs;
    }

    private static List<Type> GetConnectionsType(Assembly assembly)
    {
        Type baseConnectionType = typeof(ConnectionBuilderAbstract<,>);

        return assembly.GetTypes().Where(type =>
            !type.IsAbstract &&
            type.BaseType?.IsGenericType == true &&
            type.BaseType.GetGenericTypeDefinition() == baseConnectionType
        ).ToList();
    }

    private static List<Type> GetProducersType(Assembly assembly)
    {
        Type baseProducerType = typeof(ProducerAbstract<,>);

        return assembly.GetTypes().Where(type =>
            !type.IsAbstract &&
            type.BaseType != null &&
            type.BaseType.IsGenericType &&
            type.BaseType.GetGenericTypeDefinition() == baseProducerType
        ).ToList();
    }

    private static List<Type> GetSubscribersType(Assembly assembly)
    {
        Type baseSubscriberType = typeof(SubscriberAbstract<>);

        return assembly.GetTypes().Where(type =>
            !type.IsAbstract &&
            type.BaseType != null &&
            type.BaseType.IsGenericType &&
            type.BaseType.GetGenericTypeDefinition() == baseSubscriberType
        ).ToList();
    }

    private static List<Type> GetJobsType(Assembly assembly)
    {
        Type baseJobType = typeof(IInvocableJob<>);

        return assembly.GetTypes().Where(type =>
            type.IsClass &&
            !type.IsAbstract &&
            type.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == baseJobType
            )
        ).ToList();
    }
}
