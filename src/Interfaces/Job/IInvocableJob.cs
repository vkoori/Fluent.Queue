namespace FluentQueue.Interfaces.Job;

using System.Threading.Tasks;

public interface IInvocableJob<TMessage>
    where TMessage : class
{
    Task Invoke(TMessage message, string? correlationId);
    Task FailedJob(TMessage message, object? properties);
}
