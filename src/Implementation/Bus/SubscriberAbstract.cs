namespace FluentQueue.Implementation.Bus;

using System.Reflection;
using FluentQueue.Exceptions.RuntimeExceptions;
using FluentQueue.Interfaces.Job;
using FluentQueue.Interfaces.Queue;
using FluentQueue.Implementation.Connection;
using FluentQueue.Implementation.Helper;
using Newtonsoft.Json;

public abstract class SubscriberAbstract<TConnection>
    where TConnection : IDisposable
{
    protected readonly BusConnectionPool _connectionPool;
    protected readonly string? _connectionName;
    protected readonly int? _retry;
    private readonly BusScanner _busRequiredTypes;

    public SubscriberAbstract(
        BusConnectionPool connectionPool,
        string? connectionName,
        ushort? retry,
        BusScanner busRequiredTypes
    )
    {
        _busRequiredTypes = busRequiredTypes;
        _connectionPool = connectionPool;
        _connectionName = connectionName;
        _retry = retry;
    }

    public abstract void Consuming(List<IQueue>? queues);
    public abstract void StopConsume();

    protected void TriggerInvokeJob(string message, string? correlationId)
    {
        TriggerJobObj jobObj = TriggerJob(message: message);

        MethodInfo method = jobObj.Job.GetMethod(name: "Invoke")!;
        object jobInstance = Activator.CreateInstance(type: jobObj.Job)!;

        method.Invoke(
            obj: jobInstance,
            parameters: new object[] { jobObj.Dto, correlationId! }
        );
    }

    protected void TriggerFailedJob(string message, object? properties)
    {
        TriggerJobObj jobObj = TriggerJob(message: message);

        MethodInfo method = jobObj.Job.GetMethod(name: "FailedJob")!;
        object jobInstance = Activator.CreateInstance(type: jobObj.Job)!;

        method.Invoke(
            obj: jobInstance,
            parameters: new object[] { jobObj.Dto, properties! }
        );
    }

    private TriggerJobObj TriggerJob(string message)
    {
        List<Type> jobsType = _busRequiredTypes.GetJobs();
        Type? targetJobType = null;
        object? dto = null;

        foreach (Type jobType in jobsType)
        {
            targetJobType = jobType;
            Type jobInterfaceType = typeof(IInvocableJob<>);

            // get correct interface of job
            Type jobInterface = jobType.GetInterfaces().First(
                i => i.IsGenericType &&
                i.GetGenericTypeDefinition() == jobInterfaceType
            );

            // get target dto type
            Type jobMessageType = jobInterface.GetGenericArguments().First();

            dto = JsonConvert.DeserializeObject(message, jobMessageType);
            if (dto != null)
            {
                break;
            }
        }

        if (targetJobType == null || dto == null)
        {
            throw new JobNotFound();
        }

        return new TriggerJobObj
        {
            Job = targetJobType,
            Dto = dto
        };
    }
}
